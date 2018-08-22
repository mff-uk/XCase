using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Controller;
using XCase.Model;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace XCase.Reverse
{
    public class PSMtoPIM: INotifyPropertyChanged
    {
        #region Properties
        //tried to make it work with WPF binding to sliders and failed
        public event PropertyChangedEventHandler PropertyChanged;

        void Changed(string s)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(s));
        }
        
        /// <summary>
        /// Current PSM Diagram controller
        /// </summary>
        public Controller.DiagramController diagramController;

        double classPreMapThreshold = 0.7;
        /// <summary>
        /// Threshold for Class Pre-mapping
        /// </summary>
        public double ClassPreMapThreshold { get { return classPreMapThreshold; } set { classPreMapThreshold = value; Changed("ClassPreMapThreshold"); } }

        double classSimWeightOfString = 0.4;
        /// <summary>
        /// Weight of Class string similarity (against Class attribute similarity)
        /// </summary>
        public double ClassSimWeightOfString { get { return classSimWeightOfString; } set { classSimWeightOfString = value; Changed("ClassSimWeightOfString"); } }

        double classSimWeightOfStringAndAttr = 0.5;
        /// <summary>
        /// Weight of Class string and attribute similarity (against Class similarity adjustment)
        /// </summary>
        public double ClassSimWeightOfStringAndAttr { get { return classSimWeightOfStringAndAttr; } set { classSimWeightOfStringAndAttr = value; Changed("ClassSimWeightOfStringAndAttr"); } }

        double attrSimWeightOfString = 1;
        /// <summary>
        /// Weight of Attribute string similarity (against Attribute type similarity)
        /// </summary>
        public double AttrSimWeightOfString { get { return attrSimWeightOfString; } set { attrSimWeightOfString = value; Changed("AttrSimWeightOfString"); } }

        double attrSimWeightOfStringAndType = 0.5;
        /// <summary>
        /// Weight of Attribute string and type similarity (against Attribute similarity adjustment)
        /// </summary>
        public double AttrSimWeightOfStringAndType { get { return attrSimWeightOfStringAndType; } set { attrSimWeightOfStringAndType = value; Changed("AttrSimWeightOfStringAndType"); } }

        double nullTypesSimilarity = 0;
        /// <summary>
        /// Similarity when type == null
        /// </summary>
        public double NullTypesSimilarity { get { return nullTypesSimilarity; } set { nullTypesSimilarity = value; Changed("NullTypesSimilarity"); } }

        double mAXADJUSTMENT = double.PositiveInfinity;
        /// <summary>
        /// Maximum adjustment for unreachable PIMClasses
        /// </summary>
        public double MAXADJUSTMENT { get { return mAXADJUSTMENT; } set { mAXADJUSTMENT = value; Changed("MAXADJUSTMENT"); } }

        #endregion

        public class PIMOffer
        {
            const int RoundTo = 2;
            public double O, SC, SCsn, SCse, SCn, SCa, SCadj, W;
            public double O_ { get { return Math.Round(O, RoundTo); } }
            public double SC_ { get { return Math.Round(SC, RoundTo); } }
            public double SCsn_ { get { return Math.Round(SCsn, RoundTo); } }
            public double SCse_ { get { return Math.Round(SCse, RoundTo); } }
            public double SCn_ { get { return Math.Round(SCn, RoundTo); } }
            public double SCa_ { get { return Math.Round(SCa, RoundTo); } }
            public double SCadj_ { get { if (SCadj > 10000) return double.PositiveInfinity; else return Math.Round(SCadj, RoundTo); } }
            public double W_ { get { return Math.Round(W, RoundTo); } }
        }

        public class ClassPreMaping
        {
            const int RoundTo = 2;
            public PIMClass Class { get; set; }
            public double Similarity;
            public double Similarity_ { get { return Math.Round(Similarity, RoundTo); } }
            public bool Keep { get; set; }
        }

        #region Statistics
        double stat = 0;
        double GP = 0;
        double GPL = 0;
        double GPI = 0;
        double LP = 0;
        double LPL = 0;
        double LPI = 0;
        
        public class PIMClassStatistic
        {
            const int RoundTo = 2;
            public PSMClass PSMClass { get; set; }
            public PIMClass PIMClass { get; set; }
            public int Order  { get; set; }
            public double Similarity;
            public double Similarity_ { get { return Math.Round(Similarity, RoundTo); } }
            public int Items { get; set; }
            public int Near { get; set; }
            public double Statistic;
            public double StatisticRound { get { return Math.Round(Statistic, RoundTo); } }
            public double GP;
            public double GPRound { get { return Math.Round(GP, RoundTo); } }
            public double LP;
            public double LPRound { get { return Math.Round(LP, RoundTo); } }
            public bool CreatedNew { get; set; }
            public bool IsLeaf { get; set; }
            public bool IsPreMapped { get; set; }
            public double preMapSim;
            public double PreMapSim_ { get { return Math.Round(preMapSim, RoundTo); } }
        }

        List<PIMClassStatistic> Statistics;

        #endregion

        #region Variables
        bool log1;
        bool log2;
        bool log3;
        bool XSDtoPSM;
        bool Stat;
        bool StatFirstRun = true;
        StreamWriter SW;
        bool logtofile = true;
        
        bool cancelled = false;
        string nl = Environment.NewLine;

        int ClassCount;

        bool SSC_Direct_Children;
        bool SSC_Mapped_Ancestors;
        bool SSC_Previous_Siblings;
        bool SSC_Subtree_Leaves;
        bool SSC_Mapped_Parent;
        bool SSC_Following_Siblings;
        
        bool PreMapClasses = false;
        bool SiblingAdjustment = false;

        /// <summary>
        /// Include structural similarity with structural representants
        /// </summary>
        bool SSC_SR = false;

        /// <summary>
        /// Include name similarity with structural representants
        /// </summary>
        bool CS_NS_SR = false;

        /// <summary>
        /// Include element label similarity with structural representants
        /// </summary>
        bool CS_EL_SR = false;

        /// <summary>
        /// Include attribute similarity with structural representants
        /// </summary>
        bool ATT_SR = false;

        List<PSMClass> PSMClasses;
        List<PIMClass> PIMClasses;
        List<Property> PIMAttributes;
        List<PSMAttribute> PSMAttributes;
        List<DataType> DataTypes;

        Dictionary<PIMClass, int> idxPIMClass;
        Dictionary<PSMClass, int> idxPSMClass;
        Dictionary<Property, int> idxPIMAttribute;
        Dictionary<PSMAttribute, int> idxPSMAttribute;
        Dictionary<DataType, int> idxDataType;

        Dictionary<PSMClass, PIMClass> ClassMap, ClassPreMap;
        Dictionary<PSMClass, List<PSMClass>> SRRelation;
        Dictionary<PSMClass, ClassPreMaping> ClassPreMapOffer;
        Dictionary<PSMAttribute, Property> AttrMap;
        List<PSMClass> MappedToNewPIMClass;
        Dictionary<PSMClass, List<NestingJoinStep>> AssocMap;

        Func<string, string, double> AttributeNameStringSimilarity, AttributeTypeStringSimilarity, ClassNameStringSimilarity, ClassElementLabelStringSimilarity;
        Action<List<PSMClass>, List<PSMClass>, Dictionary<PSMClass, Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>>>, int> ComputeClassAdjustments;

        double[,] SAt, SAts, SAn, SA, SCsn, SCse, SCn, SCa, SC, SCadj;
        int PIMAttrCount, PSMAttrCount, PIMClassCount, PSMClassCount;

        PSMtoPIMWindow W;
        PSMDiagram NewPSMDiagram;
        #endregion

        public PSMtoPIM(DiagramController dC)
        {
            diagramController = dC;
            W = new PSMtoPIMWindow(this);
            W.sldAttrStringAndTypeSim.Value = AttrSimWeightOfStringAndType;
            W.sldAttrStringSim.Value = AttrSimWeightOfString;
            W.sldClassStringAndAttrSim.Value = ClassSimWeightOfStringAndAttr;
            W.sldClassStringSim.Value = ClassSimWeightOfString;
            W.chkPreMap.IsChecked = PreMapClasses;
            W.sldPreMapT.Value = ClassPreMapThreshold;
            W.chkCS_EL_SR.IsChecked = CS_EL_SR;
            W.chkCS_NS_SR.IsChecked = CS_NS_SR;
            W.chkCS_SR.IsChecked = ATT_SR;
            W.chkSRneighbors.IsChecked = SSC_SR;
        }

        void btnStart_Click(object sender, RoutedEventArgs e)
        {
            AttrSimWeightOfString = W.sldAttrStringSim.Value;
            AttrSimWeightOfStringAndType = W.sldAttrStringAndTypeSim.Value;
            ClassSimWeightOfString = W.sldClassStringSim.Value;
            ClassSimWeightOfStringAndAttr = W.sldClassStringAndAttrSim.Value;
            ClassPreMapThreshold = W.sldPreMapT.Value;
            

            log1 = W.chkLog1.IsChecked == true;
            log2 = W.chkLog2.IsChecked == true;
            log3 = W.chkLog3.IsChecked == true;
            XSDtoPSM = W.chkXSDtoPSM.IsChecked == true;
            logtofile = W.chkLogToFile.IsChecked == true;
            Stat = W.chkStat.IsChecked == true;
            PreMapClasses = W.chkPreMap.IsChecked == true;
            CS_EL_SR = W.chkCS_EL_SR.IsChecked == true;
            CS_NS_SR = W.chkCS_NS_SR.IsChecked == true;
            ATT_SR = W.chkCS_SR.IsChecked == true;
            SSC_SR = W.chkSRneighbors.IsChecked == true;

            if (W.rbAS_SS_LCSq.IsChecked == true)
            {
                AttributeNameStringSimilarity = LongestCommonSubsequence;
            }
            else if (W.rbAS_SS_LCSt.IsChecked == true)
            {
                AttributeNameStringSimilarity = LongestCommonSubstring;
            }

            if (W.rbAS_TNS_LCSq.IsChecked == true)
            {
                AttributeTypeStringSimilarity = LongestCommonSubsequence;
            }
            else if (W.rbAS_TNS_LCSt.IsChecked == true)
            {
                AttributeTypeStringSimilarity = LongestCommonSubstring;
            }

            if (W.rbCS_NS_LCSq.IsChecked == true)
            {
                ClassNameStringSimilarity = LongestCommonSubsequence;
            }
            else if (W.rbCS_NS_LCSt.IsChecked == true)
            {
                ClassNameStringSimilarity = LongestCommonSubstring;
            }

            if (W.rbCS_EL_LCSq.IsChecked == true)
            {
                ClassElementLabelStringSimilarity = LongestCommonSubsequence;
            }
            else if (W.rbCS_EL_LCSt.IsChecked == true)
            {
                ClassElementLabelStringSimilarity = LongestCommonSubstring;
            }

            if (W.rbSSDM_Avg.IsChecked == true)
            {
                ComputeClassAdjustments = ComputeClassAdjustments_Avg;
            }
            else if (W.rbSSDM_Max.IsChecked == true)
            {
                ComputeClassAdjustments = ComputeClassAdjustments_Max;
            }
            else if (W.rbSSDM_Min.IsChecked == true)
            {
                ComputeClassAdjustments = ComputeClassAdjustments_Min;
            }

            SSC_Direct_Children = W.chkSSC_DC.IsChecked == true;
            SSC_Mapped_Ancestors = W.chkSSC_MA.IsChecked == true;
            SSC_Previous_Siblings = W.chkSSC_PS.IsChecked == true;
            SSC_Subtree_Leaves = W.chkSSC_SL.IsChecked == true;
            SSC_Mapped_Parent = W.chkSSC_MP.IsChecked == true;
            SSC_Following_Siblings = W.chkSSC_FS.IsChecked == true;
            SiblingAdjustment = W.chkSibAdj.IsChecked == true;

            if (W.rbCSVcomma.IsChecked == true) CSVsep = ',';
            else if (W.rbCSVsemicolon.IsChecked == true) CSVsep = ';';

            ClassFinalStatistic.StatsEnabled[0] = W.chkStatOld.IsChecked == true;
            ClassFinalStatistic.StatsEnabled[1] = W.chkStatGP.IsChecked == true;
            ClassFinalStatistic.StatsEnabled[2] = W.chkStatLP.IsChecked == true;
            ClassFinalStatistic.StatsEnabled[3] = W.chkStatGPI.IsChecked == true;
            ClassFinalStatistic.StatsEnabled[4] = W.chkStatLPI.IsChecked == true;
            ClassFinalStatistic.StatsEnabled[5] = W.chkStatGPL.IsChecked == true;
            ClassFinalStatistic.StatsEnabled[6] = W.chkStatLPL.IsChecked == true;

            if (Stat)
            {
                StatFirstRun = true;
                ClassFinalStatistics = new ClassFinalStatistic[11, 11, 3, Combs.Length];
            }
            Algorithm();
            if (!cancelled) StatRun();
        }
        
        public void ShowWindow()
        {
            W.btnStart.Click += new RoutedEventHandler(btnStart_Click);
            W.btnClose.Click += new RoutedEventHandler(delegate { W.Close(); });
            W.Show();
        }

        List<PIMClass> GetClassesFromPackages(Package P)
        {
            List<PIMClass> L = new List<PIMClass>();

            L.AddRange(P.Classes);
            foreach (Package P2 in P.NestedPackages)
            {
                L.AddRange(GetClassesFromPackages(P2));
            }

            return L;
        }

        //to be called when PSMClasses are computed
        void GetSR_Related()
        {
            SRRelation = new Dictionary<PSMClass, List<PSMClass>>();

            foreach (PSMClass c in PSMClasses)
            {
                SRRelation[c] = new List<PSMClass>();
                SRRelation[c].Add(c);
                if (c.IsStructuralRepresentative) SRRelation[c].Add(c.RepresentedPSMClass);
            }

            bool updated = true;

            while (updated)
            {
                updated = false;
                foreach (PSMClass c in PSMClasses)
                {
                    IEnumerable<PSMClass> t = SRRelation[c].ToList<PSMClass>();
                    foreach (PSMClass c2 in t)
                    {
                        IEnumerable<PSMClass> t2 = SRRelation[c2].ToList<PSMClass>();
                        foreach (PSMClass c3 in t2)
                        {
                            if (!SRRelation[c].Contains(c3))
                            {
                                updated = true;
                                SRRelation[c].Add(c3);
                                if (!SRRelation[c3].Contains(c))
                                {
                                    SRRelation[c3].Add(c);
                                }
                            }
                        }
                    }
                }
            }

            foreach (PSMClass c in PSMClasses)
            {
                SRRelation[c].Remove(c);
            }
        }

        List<PSMClass> GetPSMDiagramClasses(PSMDiagram d)
        {
            List<PSMClass> list = new List<PSMClass>();

            foreach (PSMElement E in d.Roots)
            {
                if (E is PSMClass) list.Add(E as PSMClass);
                list = list.Concat<PSMClass>(E.GetAllPSMSubClasses()).ToList<PSMClass>();
            }

            return list;
        }
        
        void InitSimilarities()
        {
            System.Diagnostics.Stopwatch s = new Stopwatch();
            s.Start();
            
            PSMClasses = GetPSMDiagramClasses(diagramController.Diagram as PSMDiagram);

            GetSR_Related();

            PIMClasses = new List<PIMClass>();
            PIMClasses.AddRange(GetClassesFromPackages(diagramController.ModelController.Model));
            
            Print2(nl + "PIM Class and PIM Attribute indexes:" + nl + nl);

            PIMAttributes = new List<Property>();
            idxPIMClass = new Dictionary<PIMClass, int>();
            idxPIMAttribute = new Dictionary<Property, int>();
            int i = 0, j = 0;
            foreach (PIMClass pimClass in PIMClasses)
            {
                Print2("PIM Class " + i.ToString() + ": " + pimClass.Name + nl);
                idxPIMClass.Add(pimClass, i++);
                foreach (Property P in pimClass.Attributes)
                {
                    Print2("\tPIM Attribute " + j.ToString() + ": " + P.Name + nl);
                    idxPIMAttribute.Add(P, j++);
                    PIMAttributes.Add(P);
                }
            }

            Print2(nl + "PSM Class and PSM Attribute indexes:" + nl + nl);

            PSMAttributes = new List<PSMAttribute>();
            idxPSMClass = new Dictionary<PSMClass, int>();
            idxPSMAttribute = new Dictionary<PSMAttribute, int>();
            i = j = 0;
            foreach (PSMClass psmClass in PSMClasses)
            {
                Print2("PSM Class " + i.ToString() + ": " + psmClass.Name + nl);
                idxPSMClass.Add(psmClass, i++);
                foreach (PSMAttribute A in psmClass.AllPSMAttributes)
                {
                    Print2("\tPSM Attribute " + j.ToString() + ": " + A.Name + "(" + A.Alias + ")" + nl);
                    idxPSMAttribute.Add(A, j++);
                    PSMAttributes.Add(A);
                }
            }

            Print3(nl + "Datatype indexes:" + nl + nl);
            idxDataType = new Dictionary<DataType, int>();
            i = 0;
            DataTypes = new List<DataType>(diagramController.ModelController.Model.AllTypes);
            foreach (DataType t in DataTypes)
            {
                Print3("DataType " + i.ToString() + ": " + t.Name + nl);
                idxDataType.Add(t, i++);
            }

            PIMAttrCount = PIMAttributes.Count;
            PSMAttrCount = PSMAttributes.Count;
            PIMClassCount = PIMClasses.Count;
            PSMClassCount = PSMClasses.Count;

            //Matrices for attribute similarities
            SAt = new double[PSMAttrCount, PIMAttrCount];
            SAts = new double[PSMAttrCount, PIMAttrCount];
            SAn = new double[PSMAttrCount, PIMAttrCount];
            SA = new double[PSMAttrCount, PIMAttrCount];

            //Matrices for class similarities
            SCsn = new double[PSMClassCount, PIMClassCount];
            SCse = new double[PSMClassCount, PIMClassCount];
            SCn = new double[PSMClassCount, PIMClassCount];
            SCa = new double[PSMClassCount, PIMClassCount];
            SC = new double[PSMClassCount, PIMClassCount];
            SCadj = new double[PSMClassCount, PIMClassCount];

            //temp datatype similarity table - must be editable by user
            double[,] T = new double[DataTypes.Count, DataTypes.Count];
            foreach (DataType T1 in DataTypes)
                foreach (DataType T2 in DataTypes)
                {
                    if (T1 == T2) T[idxDataType[T1], idxDataType[T2]] = 1;
                    else T[idxDataType[T1], idxDataType[T2]] = 0.1;
                    //Print(T1.Name + " - " + T2.Name + ": " + T[idxDataType[T1], idxDataType[T2]].ToString() + Environment.NewLine);
                }
            
            /*Print2(nl + "Type table:" + nl + nl);
            PrintMatrix(T, DataTypes.Count, DataTypes.Count);*/

            //Attribute similarity - algorithm 2
            foreach (PSMAttribute PSMAttr in PSMAttributes)
            {
                int idxApsm = idxPSMAttribute[PSMAttr];
                foreach (Property PIMAttr in PIMAttributes)
                {
                    int idxApim = idxPIMAttribute[PIMAttr];
                    //Attribute Type Similarity
                    if (PSMAttr.Type == null || PIMAttr.Type == null)
                        SAt[idxApsm, idxApim] = NullTypesSimilarity;
                    else
                        SAt[idxApsm, idxApim] = T[idxDataType[PSMAttr.Type], idxDataType[PIMAttr.Type]];
                    //Attribute Name Similarity
                    SAn[idxApsm, idxApim] = AttributeNameStringSimilarity(PSMAttr.Alias, PIMAttr.Name);
                    if (PSMAttr.Type == null)
                        SAts[idxApsm, idxApim] = NullTypesSimilarity;
                    else
                        SAts[idxApsm, idxApim] = AttributeTypeStringSimilarity(PSMAttr.Type.Name, PIMAttr.Name);
                    //Attribute Similarity
                    SA[idxApsm, idxApim] =
                        (1 - AttrSimWeightOfString) * SAt[idxApsm, idxApim]
                        + AttrSimWeightOfString * Math.Max(SAn[idxApsm, idxApim], SAts[idxApsm, idxApim]);
                }
            }

            //Class similarity - algorithm 3a
            foreach (PSMClass psmClass in PSMClasses)
            {
                int idxCpsm = idxPSMClass[psmClass];
                foreach (PIMClass pimClass in PIMClasses)
                {
                    int idxCpim = idxPIMClass[pimClass];
                    //Similarity between names
                    SCsn[idxCpsm, idxCpim] = ClassNameStringSimilarity(psmClass.Name, pimClass.Name);
                    //Similarity between PSM Element label and PIM name
                    SCse[idxCpsm, idxCpim] = ClassElementLabelStringSimilarity(psmClass.ElementName, pimClass.Name);
                    //Final string similarity - MAX
                    SCn[idxCpsm, idxCpim] = Math.Max(SCsn[idxCpsm, idxCpim], SCse[idxCpsm, idxCpim]);
                    //Attribute similarity
                    SCa[idxCpsm, idxCpim] = CAS(psmClass, pimClass);
                    //Matrix for adjustments (so we can see before&after adjustments)
                    SCadj[idxCpsm, idxCpim] = double.PositiveInfinity;
                }
            }

            //Class similarity - algorithm 3b
            foreach (PSMClass psmClass in PSMClasses)
            {
                int idxCpsm = idxPSMClass[psmClass];
                foreach (PIMClass pimClass in PIMClasses)
                {
                    int idxCpim = idxPIMClass[pimClass];
                    double SCsnMAX = SCsn[idxCpsm, idxCpim];
                    double SCseMAX = SCse[idxCpsm, idxCpim];

                    //Get the maximum similarity of the structural representative neighborhood for final string similarity
                    //or weight and avg the neighborhood?
                    if (CS_EL_SR || CS_NS_SR)
                    {
                        foreach (PSMClass srclass in SRRelation[psmClass])
                        {
                            int idxCsrpsm = idxPSMClass[srclass];
                            if (CS_EL_SR)
                            {
                                SCseMAX = Math.Max(SCseMAX, SCse[idxCsrpsm, idxCpim]);
                            }
                            if (CS_NS_SR)
                            {
                                SCsnMAX = Math.Max(SCsnMAX, SCsn[idxCsrpsm, idxCpim]);
                            }
                        }
                    }
                    //Final string similarity - MAX
                    SCn[idxCpsm, idxCpim] = Math.Max(SCsnMAX, SCseMAX);

                    //final similarity
                    SC[idxCpsm, idxCpim] = ClassSimWeightOfString * SCn[idxCpsm, idxCpim] + (1 - ClassSimWeightOfString) * SCa[idxCpsm, idxCpim];
                }
            }

            s.Stop();
            Print2("Computing initial similarities took: " + s.ElapsedMilliseconds.ToString() + "ms" + nl);
        }

        void ClassPreMapping()
        {
            ClassPreMapOffer = new Dictionary<PSMClass, ClassPreMaping>();
            for (int i = 0; i < PSMClassCount; i++)
            {
                double MaxSC = 0;
                PIMClass MaxPIMClass = null;
                for (int j = 0; j < PIMClassCount; j++)
                {
                    if (SC[i, j] > MaxSC)
                    {
                        MaxSC = SC[i, j];
                        MaxPIMClass = PIMClasses[j];
                    }
                }
                if (MaxSC >= ClassPreMapThreshold && MaxPIMClass != null)
                {
                    ClassPreMapOffer.Add(PSMClasses[i], new ClassPreMaping() { Class = MaxPIMClass, Similarity = MaxSC, Keep = true });
                    //SR? should work automatically, all classes of a SR group should have the same similarity levels
                }
            }
            if (!Stat || StatFirstRun)
            {
                if (ClassPreMapOffer.Count == 0) return;
                ClassPreMapView W = new ClassPreMapView();
                W.Mapping = ClassPreMapOffer;

                if (W.ShowDialog() != true)
                {
                    cancelled = true;
                    return;
                }
            }
            foreach (KeyValuePair<PSMClass, ClassPreMaping> P in ClassPreMapOffer)
            {
                if (P.Value.Keep)
                {
                    //ClassMap.Add(P.Key, P.Value.Class);
                    ClassPreMap.Add(P.Key, P.Value.Class);
                }
            }
        }
        
        void Algorithm()
        {
            #region Initial logging
            if (logtofile) SW = new StreamWriter("log.txt");
            cancelled = false;
            Print("Algorithm starting" + nl + "=====================" + nl);
            Print("Weight of Class string similarity: " + ClassSimWeightOfString.ToString() + nl);
            Print("Weight of Class string and attribute similarity: " + ClassSimWeightOfStringAndAttr.ToString() + nl);
            Print("Weight of Attribute string similarity: " + AttrSimWeightOfString.ToString() + nl);
            Print("Weight of Attribute string and type similarity: " + AttrSimWeightOfStringAndType.ToString() + nl);
            Print("Null Types Similarity: " + NullTypesSimilarity.ToString() + nl);
            Print("Maximum adjustment: " + MAXADJUSTMENT.ToString() + nl);
            Print("Class Name String Similarity Method: " + ClassNameStringSimilarity.Method.Name + nl);
            Print("Element Label String Similarity Method: " + ClassElementLabelStringSimilarity.Method.Name + nl);
            Print("Attribute String Similarity Method: " + AttributeNameStringSimilarity.Method.Name + nl);
            Print("Attribute Type String Similarity Method: " + AttributeTypeStringSimilarity.Method.Name + nl);
            Print("Class Structural Similarity Distance Combination Method: " + ComputeClassAdjustments.Method.Name + nl);
            Print("Class Structural Similarity Distance Adjustment for Siblings: " + SiblingAdjustment + nl);
            Print("Class Structural Similarity Candidates: "
                + (SSC_Direct_Children ? "Direct children" : "")
                + (SSC_Mapped_Ancestors ? " + Mapped Ancestors" : "")
                + (SSC_Previous_Siblings ? " + Previous siblings" : "")
                + (SSC_Subtree_Leaves ? " + Subtree leaves" : "")
                + (SSC_Mapped_Parent ? " + Mapped parent" : "")
                + (SSC_Following_Siblings ? " + Following siblings" : "")
                + nl);
            Print("Premapping: " + (PreMapClasses ? "Yes: " + ClassPreMapThreshold.ToString() : "No") + nl);
            #endregion

            ClassCount = 1;
            if (!Stat || StatFirstRun)
            {
                ClassMap = new Dictionary<PSMClass, PIMClass>();
                AttrMap = new Dictionary<PSMAttribute, Property>();
                AssocMap = new Dictionary<PSMClass, List<NestingJoinStep>>();
                MappedToNewPIMClass = new List<PSMClass>();
            }
            ClassPreMap = new Dictionary<PSMClass, PIMClass>();
            W.textBlock.Text = "";

            InitSimilarities(); //algorithms 2 and 3

            if (PreMapClasses) ClassPreMapping();

            if (cancelled)
            {
                Print("Algorithm cancelled." + nl);
                if (logtofile) SW.Close();
                return;
            }

            Statistics = new List<PIMClassStatistic>();
            //Class mapping - algorithm 4
            foreach (PSMSuperordinateComponent root in (diagramController.Diagram as PSMDiagram).Roots)
            {
                //Traversal of the PSMtree post-order
                PSMClass rootclass = root as PSMClass;
                if (rootclass == null) continue;
                ClassMapping(rootclass);
            }

            PrintClassMapping();

            #region Final statistics
            stat = GP = GPL = GPI = LP = LPL = LPI = 0;
            foreach (PIMClassStatistic S in Statistics)
            {
                stat += S.Statistic;
                GP += S.GP;
                LP += S.LP;
                if (S.IsLeaf)
                {
                    GPL += S.GP;
                    LPL += S.LP;
                }
                else
                {
                    GPI += S.GP;
                    LPI += S.LP;
                }
            }
            double mapped = Statistics.Where<PIMClassStatistic>(I => I.CreatedNew == false).Count<PIMClassStatistic>();
            if (mapped == 0) mapped = double.MaxValue;
            double mappedL = Statistics.Where<PIMClassStatistic>(I => I.CreatedNew == false && I.IsLeaf).Count<PIMClassStatistic>();
            if (mappedL == 0) mappedL = double.MaxValue;
            double mappedI = Statistics.Where<PIMClassStatistic>(I => I.CreatedNew == false && !I.IsLeaf).Count<PIMClassStatistic>();
            if (mappedI == 0) mappedI = double.MaxValue;
            
            stat /= mapped;
            GP /= mapped;
            LP /= mapped;
            GPL /= mappedL;
            LPL /= mappedL;
            GPI /= mappedI;
            LPI /= mappedI;

            if (!Stat && !cancelled)
            {
                PIMClassStatistics StatWindow = new PIMClassStatistics();
                StatWindow.lbx.DataContext = Statistics;
                StatWindow.lblStat.Content = stat.ToString();
                StatWindow.lblGP.Content = GP.ToString();
                StatWindow.lblLP.Content = LP.ToString();
                StatWindow.lblGPL.Content = GPL.ToString();
                StatWindow.lblLPL.Content = LPL.ToString();
                StatWindow.lblGPI.Content = GPI.ToString();
                StatWindow.lblLPI.Content = LPI.ToString();
                StatWindow.lblClasses.Content = mapped.ToString();
                StatWindow.lblClassesL.Content = mappedL.ToString();
                StatWindow.lblClassesI.Content = mappedI.ToString();
                StatWindow.Show();
            }
            #endregion

            if (cancelled)
            {
                Print("Algorithm cancelled." + nl);
                if (logtofile) SW.Close();
                return;
            }

            //Temporary until attribute statistics are done
            if (Stat) return;

            AttributeMapping();

            if (cancelled) Print("Algorithm cancelled." + nl);

            if (!Stat)
            {
                //Asks whether to generate a mapped PSM Diagram or not
                MessageBoxResult r = MessageBox.Show("Generate a mapped PSM?", "XCase", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
                if (r == MessageBoxResult.Yes)
                {
                    string NewPSMDiagramName = NameSuggestor<PSMDiagram>.IsNameUnique(diagramController.ModelController.Project.PSMDiagrams, diagramController.Diagram.Caption + " (mapped)", diagram => diagram.Caption)
                        ? diagramController.Diagram.Caption + " (mapped)"
                        : NameSuggestor<PSMDiagram>.SuggestUniqueName(diagramController.ModelController.Project.PSMDiagrams, diagramController.Diagram.Caption + " (mapped)", diagram => diagram.Caption);
                    NewPSMDiagram = new PSMDiagram(NewPSMDiagramName);
                    diagramController.ModelController.Project.AddPSMDiagram(NewPSMDiagram);
                    foreach (PSMSuperordinateComponent Sup in (diagramController.Diagram as PSMDiagram).Roots)
                        RegeneratePSM(Sup, null, null);
                }
                else if (r == MessageBoxResult.Cancel)
                {
                    cancelled = true;
                }
            }
            
            if (cancelled) Print("Algorithm cancelled." + nl);
            else Print("Algorithm ended." + nl);
            
            if (logtofile) SW.Close();
        }

        void AttributeMapping()
        {
            //Attribute mapping - algorithm 5
            int count = 1;
            foreach (PSMAttribute psmAttr in PSMAttributes)
            {
                int idxApsm = idxPSMAttribute[psmAttr];
                double[] tempsim = new double[PIMAttrCount];
                List<KeyValuePair<Property, double>> Offer = new List<KeyValuePair<Property, double>>();
                Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>> d = PIMBFS.BFS(ClassMap[psmAttr.Class]);
                foreach (Property pimAttr in PIMAttributes)
                {
                    int idxApim = idxPIMAttribute[pimAttr];
                    tempsim[idxApim] = AttrSimWeightOfStringAndType * SA[idxApsm, idxApim];
                    if (!ClassMap[psmAttr.Class].AllAttributes.Contains(pimAttr))
                    {
                        //pimAttribute of another PIM class than the one to wich the PSM Class of the current PSM Attribute is mapped to
                        double Adjustment;
                        if (d.ContainsKey(pimAttr.Class as PIMClass))
                        {
                            Adjustment = d[pimAttr.Class as PIMClass].Item1;
                        }
                        else
                        {
                            //The actual PIM class of the PIM Attribute is unreachable
                            Adjustment = MAXADJUSTMENT;
                        }
                        tempsim[idxApim] += (1 - AttrSimWeightOfStringAndType) * 1 / (1 + Adjustment);
                        Print2("Adj PSMAtt " + psmAttr.Class.Name + "." + psmAttr.Name + "(" + psmAttr.Alias + ") PIMAtt " + pimAttr.Class.Name + "." + pimAttr.Name + ". D: " + Adjustment.ToString() + " Sim: " + String.Format("{0:0.00}", tempsim[idxApim]) + nl);
                    }
                    else
                        tempsim[idxPIMAttribute[pimAttr]] += 1 - AttrSimWeightOfStringAndType;

                    Offer.Add(new KeyValuePair<Property, double>(pimAttr, Math.Round(tempsim[idxApim], 2)));
                    Offer = Offer.OrderByDescending<KeyValuePair<Property, double>, double>(P => P.Value).ToList<KeyValuePair<Property, double>>();
                }

                Print2(nl);
                OfferPIMAttributeWindow W = new OfferPIMAttributeWindow();
                W.Offer = Offer;
                W.lblLabel.Content = "Select mapping for PSM Attribute " + psmAttr.Class.Name + "." + psmAttr.Name + "(" + psmAttr.Alias + ")" + " (" + count.ToString() + "/" + PSMAttrCount + "):";
                bool? result = W.ShowDialog();
                if (result == true)
                {
                    if (W.NewPIM || W.Selected == null)
                    {
                        NewAttributeCommand c = NewAttributeCommandFactory.Factory().Create(diagramController.ModelController) as NewAttributeCommand;
                        c.Owner = ClassMap[psmAttr.Class];
                        c.Name = NameSuggestor<Property>.IsNameUnique(ClassMap[psmAttr.Class].AllAttributes, psmAttr.Name, P => P.Name)
                            ? psmAttr.Name
                            : NameSuggestor<Property>.SuggestUniqueName(ClassMap[psmAttr.Class].AllAttributes, psmAttr.Name, P => P.Name);
                        c.Lower = psmAttr.Lower;
                        c.Upper = psmAttr.Upper;
                        c.Default = psmAttr.Default;
                        c.Type = new ElementHolder<DataType>(psmAttr.Type);
                        c.Execute();
                        Property NewPIMAttr = c.createdAttribute;
                        AttrMap.Add(psmAttr, NewPIMAttr);
                        Print2("Mapped PSM Attribute " + psmAttr.Class.Name + "." + psmAttr.Name + "(" + psmAttr.Alias + ") to a new PIM attribute " + NewPIMAttr.Class.Name + "." + NewPIMAttr.Name + nl);
                        Print2(nl + "PIM attribute added, recomputing initial similarities." + nl + nl);
                        InitSimilarities();
                    }
                    else
                    {
                        if (!Stat || StatFirstRun) AttrMap.Add(psmAttr, W.Selected);
                        Print2("Mapped PSM Attribute " + psmAttr.Class.Name + "." + psmAttr.Name + "(" + psmAttr.Alias + ") to " + W.Selected.Class.Name + "." + W.Selected.Name + nl);
                    }
                    count++;
                }
                else
                {
                    cancelled = true;
                    return;
                }
            }
        }

        #region Structural Similarity Distance Combination Methods
        void ComputeClassAdjustments_Avg(List<PSMClass> Candidates, List<PSMClass> Siblings, Dictionary<PSMClass, Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>>> Ds, int idxCpsm)
        {
            for (int i = 0; i < PIMClassCount; i++) SCadj[idxCpsm, i] = 0;
            //Compile the results into Dj for each PIMClass
            foreach (PSMClass C in Candidates)
                foreach (PIMClass pimClass in PIMClasses)
                {
                    int idxCpim = idxPIMClass[pimClass];
                    //if pimClass is reachable from PIMClass mapped to Child C, add distance, else add MAX
                    if (Ds[C].ContainsKey(pimClass))
                    {
                        int adj = Ds[C][pimClass].Item1;
                        if (SiblingAdjustment)
                        {
                            //Sibling's best distance is 2 (through parent). Dist = 1 gets penalty, dist > 1 gets bonus
                            if (Siblings.Contains(C) && adj > 1) adj--;
                            else adj += 1;
                        }
                        SCadj[idxCpsm, idxCpim] += adj;
                    }
                    else
                    {
                        SCadj[idxCpsm, idxCpim] += MAXADJUSTMENT;
                    }
                }

            //Normalize by children count
            int count = Candidates.Count;
            foreach (PIMClass pimClass in PIMClasses)
            {
                int idxCpim = idxPIMClass[pimClass];
                SCadj[idxCpsm, idxCpim] /= count;
                Print2("PIM Class " + pimClass.Name + " adjustment: " + String.Format("{0:0.00}\n", SCadj[idxCpsm, idxCpim]));
            }
        }

        void ComputeClassAdjustments_Min(List<PSMClass> Candidates, List<PSMClass> Siblings, Dictionary<PSMClass, Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>>> Ds, int idxCpsm)
        {
            for (int i = 0; i < PIMClassCount; i++) SCadj[idxCpsm, i] = MAXADJUSTMENT;
            //Compile the results into Dj for each PIMClass
            foreach (PSMClass C in Candidates)
                foreach (PIMClass pimClass in PIMClasses)
                {
                    int idxCpim = idxPIMClass[pimClass];
                    //if pimClass is reachable from PIMClass mapped to Child C, pick minimum
                    if (Ds[C].ContainsKey(pimClass))
                    {
                        int adj = Ds[C][pimClass].Item1;
                        if (SiblingAdjustment)
                        {
                            //Sibling's best distance is 2 (through parent). Dist = 1 gets penalty, dist > 1 gets bonus
                            if (Siblings.Contains(C) && adj > 1) adj--;
                            else adj += 1;
                        }
                        if (adj < SCadj[idxCpsm, idxCpim]) SCadj[idxCpsm, idxCpim] = adj;
                    }
                }
        }

        void ComputeClassAdjustments_Max(List<PSMClass> Candidates, List<PSMClass> Siblings, Dictionary<PSMClass, Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>>> Ds, int idxCpsm)
        {
            for (int i = 0; i < PIMClassCount; i++) SCadj[idxCpsm, i] = 0;
            //Compile the results into Dj for each PIMClass
            foreach (PSMClass C in Candidates)
                foreach (PIMClass pimClass in PIMClasses)
                {
                    int idxCpim = idxPIMClass[pimClass];
                    //if pimClass is reachable from PIMClass mapped to Child C, pick minimum
                    if (Ds[C].ContainsKey(pimClass))
                    {
                        int adj = Ds[C][pimClass].Item1;
                        if (SiblingAdjustment)
                        {
                            //Sibling's best distance is 2 (through parent). Dist = 1 gets penalty, dist > 1 gets bonus
                            if (Siblings.Contains(C) && adj > 1) adj--;
                            else adj += 1;
                        }
                        if (adj > SCadj[idxCpsm, idxCpim]) SCadj[idxCpsm, idxCpim] = adj;
                    }
                    else
                    {
                        SCadj[idxCpsm, idxCpim] = MAXADJUSTMENT;
                    }
                }
        }
        #endregion

        /// <summary>
        /// Controls the process of mapping PSM Class <paramref name="psmClass"/> to a PIM Class
        /// </summary>
        /// <param name="psmClass">PSM Class to be mapped</param>
        void ClassMapping(PSMClass psmClass)
        {
            int idxCpsm = idxPSMClass[psmClass];

            //Recursion
            List<PSMClass> Children = psmClass.GetDirectPSMSubClasses();
            foreach (PSMClass C in Children) ClassMapping(C);

            //Detect if user cancelled the algorithm
            if (cancelled) return;

            #region Creating a list of candidates for structural similarity adjustment
            List<PSMClass> SSCandidates = new List<PSMClass>();
            List<PSMClass> SSCSiblings = new List<PSMClass>();
            if (SSC_Direct_Children) SSCandidates.AddRange(Children);
            if (SSC_Subtree_Leaves) SSCandidates.AddRange(psmClass.GetAllPSMClassLeaves());
            if (SSC_Mapped_Ancestors) SSCandidates.AddRange(psmClass.GetAllPSMClassAncestors().Where<PSMClass>(C => ClassPreMap.ContainsKey(C)));
            if (SSC_Previous_Siblings)
            {
                IEnumerable<PSMClass> Siblings = psmClass.GetAllPSMClassPreviousSiblings();
                SSCandidates.AddRange(Siblings);
                SSCSiblings.AddRange(Siblings);
            }
            if (SSC_Following_Siblings)
            {
                IEnumerable<PSMClass> Siblings = psmClass.GetAllPSMClassFollowingSiblings().Where<PSMClass>(C => ClassPreMap.ContainsKey(C));
                SSCandidates.AddRange(Siblings);
                SSCSiblings.AddRange(Siblings);
            }
            if (SSC_Mapped_Parent)
            {
                PSMClass parent = psmClass.GetPSMClassParent();
                if ( parent != null && ClassPreMap.ContainsKey(parent))
                    SSCandidates.Add(parent);
            }
            //Add structural representative related classes neighborhood
            if (SSC_SR)
            {
                foreach (PSMClass sr in SRRelation[psmClass])
                {
                    //Only mapped ones
                    List<PSMClass> srChildren = sr.GetDirectPSMSubClasses();
                    if (SSC_Direct_Children) SSCandidates.AddRange(srChildren.Where<PSMClass>(C => ClassPreMap.ContainsKey(C)));
                    if (SSC_Subtree_Leaves) SSCandidates.AddRange(sr.GetAllPSMClassLeaves().Where<PSMClass>(C => ClassPreMap.ContainsKey(C)));
                    if (SSC_Mapped_Ancestors) SSCandidates.AddRange(sr.GetAllPSMClassAncestors().Where<PSMClass>(C => ClassPreMap.ContainsKey(C)));
                    if (SSC_Previous_Siblings)
                    {
                        IEnumerable<PSMClass> Siblings = sr.GetAllPSMClassPreviousSiblings().Where<PSMClass>(C => ClassPreMap.ContainsKey(C));
                        SSCandidates.AddRange(Siblings);
                        SSCSiblings.AddRange(Siblings);
                    }
                    if (SSC_Following_Siblings)
                    {
                        IEnumerable<PSMClass> Siblings = sr.GetAllPSMClassFollowingSiblings().Where<PSMClass>(C => ClassPreMap.ContainsKey(C)).ToList<PSMClass>();
                        SSCandidates.AddRange(Siblings);
                        SSCSiblings.AddRange(Siblings);
                    }
                    if (SSC_Mapped_Parent)
                    {
                        PSMClass parent = sr.GetPSMClassParent();
                        if (parent != null && ClassPreMap.ContainsKey(parent))
                            SSCandidates.Add(parent);
                    }
                }
            }
            
            SSCandidates = SSCandidates.Distinct<PSMClass>().ToList<PSMClass>();
            SSCSiblings = SSCSiblings.Distinct<PSMClass>().ToList<PSMClass>();
            #endregion

            Dictionary<PSMClass, Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>>> Ds = new Dictionary<PSMClass, Dictionary<PIMClass, Tuple<int, List<NestingJoinStep>>>>();
            //If not a leaf - compute adjustment to SCadj
            if (SSCandidates.Count > 0)
            {
                //Compute distances from all PIMClasses to which Children are mapped to to all PIMClasses including corresponding PIMPaths
                foreach (PSMClass C in SSCandidates) Ds.Add(C, PIMBFS.BFS(ClassMap[C]));
                ComputeClassAdjustments(SSCandidates, SSCSiblings, Ds, idxCpsm);
            }

            //Offer SC/adj to the user for mapping
            List<Tuple<PIMClass, PIMOffer>> Offer = new List<Tuple<PIMClass, PIMOffer>>();
            foreach (PIMClass pimClass in PIMClasses)
            {
                int idxCpim = idxPIMClass[pimClass];
                PIMOffer O = new PIMOffer()
                {
                    SC = SC[idxCpsm, idxCpim],
                    SCa = SCa[idxCpsm, idxCpim],
                    SCn = SCn[idxCpsm, idxCpim],
                    SCse = SCse[idxCpsm, idxCpim],
                    SCsn = SCsn[idxCpsm, idxCpim],
                    SCadj = SCadj[idxCpsm, idxCpim],
                    W = ClassSimWeightOfStringAndAttr
                };
                O.O = O.W * O.SC + (1 - O.W) * 1 / (/*1 + */O.SCadj);

                Offer.Add(new Tuple<PIMClass, PIMOffer>(pimClass, O));
            }
            Offer = Offer.OrderByDescending<Tuple<PIMClass, PIMOffer>, double>(P => P.Item2.O).ToList<Tuple<PIMClass, PIMOffer>>();
            
            PIMClass Selected;
            bool NewPIM = false;
            bool? result;
            if (ClassPreMap.ContainsKey(psmClass) && (!Stat || StatFirstRun))
            {
                //Pre-mapped class and not running statistics
                result = true;
                Selected = ClassPreMap[psmClass];
            }
            else if ((Stat && !StatFirstRun))
            {
                //Stat run
                result = true;
                Selected = ClassMap[psmClass];
            }
            else
            {
                OfferPIMClassWindow W = new OfferPIMClassWindow();
                W.lblLabel.Content = "Select mapping for " + psmClass.Name + " (" + ClassCount.ToString() + "/" + PSMClassCount + "):";
                Print3("Mapping " + psmClass.Name + nl + nl);
                W.Offer = Offer;
                result = W.ShowDialog();
                NewPIM = W.NewPIM;
                Selected = W.Selected;
            }

            if (result == true && NewPIM == false)
            {
                //User selected a PIMClass to map to
                if ((!Stat || StatFirstRun))
                {
                    ClassMap.Add(psmClass, Selected);
                }
                Print3("PSM Class " + psmClass.Name + " mapped to PIMClass " + Selected.Name + nl);

                #region Statistic computing
                PIMClassStatistic S = new PIMClassStatistic();
                S.IsLeaf = Children.Count == 0;
                S.IsPreMapped = ClassPreMap.ContainsKey(psmClass);
                if (S.IsPreMapped) S.preMapSim = ClassPreMapOffer[psmClass].Similarity;
                S.PSMClass = psmClass;
                S.PIMClass = Selected;
                S.CreatedNew = false;
                S.Items = Offer.Count;
                if (XSDtoPSM) S.Items--; //-1 for XSD->PSM temp PIMClass
                foreach (Tuple<PIMClass, PIMOffer> P in Offer)
                {
                    if (P.Item1 == Selected)
                    {
                        S.Order = Offer.IndexOf(P) + 1;
                        S.Similarity = P.Item2.O;
                        while (S.Order < Offer.Count)
                        {
                            if (Offer[S.Order].Item2.O == S.Similarity) S.Order++;
                            else break;
                        }
                    }
                }
                S.Near = 0;
                foreach (Tuple<PIMClass, PIMOffer> P in Offer)
                {
                    if (Math.Abs(S.Similarity - P.Item2.O) < 0.1) S.Near++;
                }
                if (S.Items == 0)
                {
                    S.Statistic = 0;
                    S.GP = 0;
                    S.LP = 0;
                }
                else
                {
                    S.Statistic = 1 - (S.Order - 1) / (double)(S.Items * 2) - (S.Near - 1) / (double)(S.Items * 2);
                    S.GP = 1 - Math.Sqrt((S.Order - 1) / (double)S.Items);
                    S.LP = 1 - (S.Near - 1) / (double)S.Items;
                }
                Statistics.Add(S);
                #endregion

                if (!Stat || StatFirstRun) foreach (PSMClass C in Children)
                    {
                        //Discover children mapped to new PIM classes and add PIM associations
                        if (MappedToNewPIMClass.Contains(C))
                        {
                            NewModelAssociationCommand c2 = NewModelAssociationCommandFactory.Factory().Create(diagramController.ModelController) as NewModelAssociationCommand;
                            List<PIMClass> AssociatedClasses = new List<PIMClass>();
                            AssociatedClasses.Add(ClassMap[C]);
                            AssociatedClasses.Add(Selected);
                            c2.AssociatedClasses = AssociatedClasses.Cast<Class>();
                            c2.Execute();

                            AssocMap.Add(C, new List<NestingJoinStep>());
                            AssocMap[C].Add(new NestingJoinStep() { Association = c2.CreatedAssociation.Element, Start = ClassMap[C], End = Selected });
                        }
                        else
                        {
                            //Class mapped to an existing PIMClass, store the PIMPath
                            if (!Ds.ContainsKey(C))
                            {
                                AssocMap.Add(C, PIMBFS.BFS(ClassMap[C])[Selected].Item2);
                            }
                            else
                            {
                                if (Ds[C].ContainsKey(Selected))
                                {
                                    AssocMap.Add(C, Ds[C][Selected].Item2);
                                }
                                else
                                {
                                    //Mapped to existing PIMClass, but there is no PIMPath between them => create one
                                    NewModelAssociationCommand c2 = NewModelAssociationCommandFactory.Factory().Create(diagramController.ModelController) as NewModelAssociationCommand;
                                    List<PIMClass> AssociatedClasses = new List<PIMClass>();
                                    AssociatedClasses.Add(ClassMap[C]);
                                    AssociatedClasses.Add(Selected);
                                    c2.AssociatedClasses = AssociatedClasses.Cast<Class>();
                                    c2.Execute();

                                    AssocMap.Add(C, new List<NestingJoinStep>());
                                    AssocMap[C].Add(new NestingJoinStep() { Association = c2.CreatedAssociation.Element, Start = ClassMap[C], End = Selected });
                                }
                            }
                        }
                    }
            }
            else if (result == true && (NewPIM == true || Selected == null))
            {
                //User wants to create a new PIMClass

                //Create one, map
                NewModelClassCommand c1 = NewModelClassCommandFactory.Factory().Create(diagramController.ModelController) as NewModelClassCommand;
                c1.ClassName = NameSuggestor<PIMClass>.IsNameUnique(diagramController.ModelController.Model.Classes, psmClass.Name, C=> C.Name)
                    ? psmClass.Name
                    : NameSuggestor<PIMClass>.SuggestUniqueName(diagramController.ModelController.Model.Classes, psmClass.Name, C => C.Name);
                c1.Package = diagramController.ModelController.Model;
                c1.Execute();
                PIMClass NewPIMClass = c1.CreatedClass.Element;
                ClassMap.Add(psmClass, NewPIMClass);
                MappedToNewPIMClass.Add(psmClass);
                Print3("PSM Class " + psmClass.Name + " mapped to a new PIMClass " + NewPIMClass.Name + nl);

                #region Statistic computing
                PIMClassStatistic S = new PIMClassStatistic();
                S.IsLeaf = Children.Count == 0;
                S.IsPreMapped = false;
                S.PSMClass = psmClass;
                S.PIMClass = NewPIMClass;
                S.Items = Offer.Count;
                if (XSDtoPSM) S.Items--; //-1 for XSD->PSM temp PIMClass
                S.Order = 0;
                S.Similarity = 0;
                S.Near = 0;
                S.Statistic = 0;
                S.GP = 0;
                S.LP = 0;
                S.CreatedNew = true;
                Statistics.Add(S);
                #endregion

                //Create PIM associations from the new PIMClass to PIMClasses to which the children are mapped to
                foreach (PSMClass C in Children)
                {
                    NewModelAssociationCommand c2 = NewModelAssociationCommandFactory.Factory().Create(diagramController.ModelController) as NewModelAssociationCommand;
                    List<PIMClass> AssociatedClasses = new List<PIMClass>();
                    AssociatedClasses.Add(ClassMap[C]);
                    AssociatedClasses.Add(NewPIMClass);
                    c2.AssociatedClasses = AssociatedClasses.Cast<Class>();
                    c2.Execute();

                    AssocMap.Add(C, new List<NestingJoinStep>());
                    AssocMap[C].Add(new NestingJoinStep() { Association = c2.CreatedAssociation.Element, Start = ClassMap[C], End = NewPIMClass });
                }

                //Recompute initial similarities with the new PIM classes
                Print2(nl + "PIM Class added. Recomputing similarities." + nl + nl);
                InitSimilarities();
            }
            else
            {
                cancelled = true;
                return;
            }
            ClassCount++;
        }

        #region String Similarity Computing

        public double LongestCommonSubsequence(string s1, string s2)
        {
            //modified code from http://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Longest_common_subsequence
            //if either string is empty, the length must be 0
            if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2))
                return 0;

            int[,] num = new int[s1.Length, s2.Length];  //2D array
            char letter1;
            char letter2;

            //Actual algorithm
            for (int i = 0; i < s1.Length; i++)
            {
                letter1 = s1[i];
                for (int j = 0; j < s2.Length; j++)
                {
                    letter2 = s2[j];

                    if (letter1 == letter2)
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];
                    }
                    else
                    {
                        if ((i == 0) && (j == 0))
                            num[i, j] = 0;
                        else if ((i == 0) && !(j == 0))   //First ith element
                            num[i, j] = Math.Max(0, num[i, j - 1]);
                        else if (!(i == 0) && (j == 0))   //First jth element
                            num[i, j] = Math.Max(num[i - 1, j], 0);
                        else // if (!(i == 0) && !(j == 0))
                            num[i, j] = Math.Max(num[i - 1, j], num[i, j - 1]);
                    }
                }//end j
            }//end i

            return num[s1.Length - 1, s2.Length - 1] / (double)Math.Max(s1.Length, s2.Length);
        } //end LongestCommonSubsequence

        public double LongestCommonSubstring(string str1, string str2)
        {
            //modified code from http://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Longest_common_substring
            if (String.IsNullOrEmpty(str1) || String.IsNullOrEmpty(str2))
                return 0;

            int[,] num = new int[str1.Length, str2.Length];
            int maxlen = 0;

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                        num[i, j] = 0;
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                        }
                    }
                }
            }
            return (double)maxlen / Math.Max(str1.Length, str2.Length);
        }

        #endregion

        double CAS(PSMClass psmClass, PIMClass pimClass)
        {
            double result = 0;
            List<PSMAttribute> atts = new List<PSMAttribute>();
            atts.AddRange(psmClass.AllPSMAttributes);
            
            //if considering attributes of structural representants
            if (ATT_SR)
            {
                foreach (PSMClass c in SRRelation[psmClass])
                {
                    atts.AddRange(c.AllPSMAttributes);
                }
            }

            for (int k = 0; k < atts.Count; k++)
            {
                result += MAS(atts[k], pimClass);
            }
            //Normalized
            return atts.Count == 0 ? 0 : result / atts.Count;
        }
        double MAS(PSMAttribute psmAttribute, PIMClass pimClass)
        {
            double max = 0;
            foreach (Property PIMAttr in pimClass.Attributes)
            {
                max = Math.Max(SA[idxPSMAttribute[psmAttribute], idxPIMAttribute[PIMAttr]], max);
            }

            return max;
        }

        #region Text Output Support
        void Print3(string s)
        {
            if (log3 && !Stat)
            {
                W.textBlock.Text += s;
            }
            if (logtofile) SW.Write(s);
        }
        void Print2(string s)
        {
            if (log2 && !Stat)
            {
                W.textBlock.Text += s;
            }
            if (logtofile) SW.Write(s);
        }
        void Print(string s)
        {
            if (log1 && !Stat)
            {
                W.textBlock.Text += s;
            }
            if (logtofile) SW.Write(s);
        }

        void PrintMatrix(double[,] matrix, int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    Print2(String.Format("{0:0.00} ", matrix[i, j]));
                }
                Print2(nl);
            }
        }

        private void PrintClassMapping()
        {
            Print(nl + "Class mapping summary:" + nl + nl);
            foreach (KeyValuePair<PSMClass, PIMClass> M in ClassMap)
            {
                Print(M.Key.Name + " -> " + M.Value.Name + nl);
            }
        }

        #endregion

        void RegeneratePSM(PSMSubordinateComponent current, PSMSuperordinateComponent parent)
        {
            if (current is PSMSuperordinateComponent) RegeneratePSM(current as PSMSuperordinateComponent, parent, null);
            else if (current is PSMAttributeContainer)
            {
                PSMAttributeContainer old = current as PSMAttributeContainer;
                
                PSMClass owner = null;
                PSMSuperordinateComponent PSMSuper = null;
                PSMAttributeContainer psmAttributeContainer = null;
                if (parent is PSMClass)
                {
                    owner = parent as PSMClass;
                }
                else
                {
                    PSMSuper = parent;
                    PSMSuperordinateComponent p = parent;
                    while (!(p is PSMClass)) p = (p as PSMSubordinateComponent).Parent;
                    owner = p as PSMClass;
                }
                
                List<PSMAttribute> PSMAttributes = new List<PSMAttribute>();
                foreach (PSMAttribute A in old.PSMAttributes)
                {
                    PSMAttribute At = owner.AddAttribute() as PSMAttribute;
                    At.Name = A.Alias;
                    At.Lower = A.Lower;
                    At.Type = A.Type;
                    At.Upper = A.Upper;
                    At.Default = A.Default;
                    At.Alias = A.Alias;
                    PSMAttributes.Add(At);
                    if (AttrMap.ContainsKey(A))
                    {
                        At.RepresentedAttribute = AttrMap[A];
                        AttrMap[A].DerivedPSMAttributes.Add(At);
                    }
                    owner.PSMAttributes.Remove(At);
                }

                if (PSMSuper != null) psmAttributeContainer = (PSMAttributeContainer)PSMSuper.AddComponent(PSMAttributeContainerFactory.Instance);
                else psmAttributeContainer = (PSMAttributeContainer)owner.AddComponent(PSMAttributeContainerFactory.Instance);
                
                foreach (PSMAttribute attribute in PSMAttributes) psmAttributeContainer.PSMAttributes.Add(attribute);
                NewPSMDiagram.AddModelElement(psmAttributeContainer, new PSMElementViewHelper(NewPSMDiagram));

                foreach (Comment C in old.Comments)
                {
                    Comment NewC = psmAttributeContainer.AddComment(NameSuggestor<Comment>.SuggestUniqueName(current.Comments, "Comment", comment => comment.Body));
                    NewC.Body = C.Body;
                    NewPSMDiagram.AddModelElement(NewC, new CommentViewHelper(NewPSMDiagram));
                }

            }
            else if (current is PSMAssociation)
            {
                RegeneratePSM((current as PSMAssociation).Child as PSMClass, parent, current as PSMAssociation);
            }
        }

        void RegeneratePSM(PSMSuperordinateComponent current, PSMSuperordinateComponent parent, PSMAssociation oldParentAssociation)
        {
            PSMSuperordinateComponent NewComponent;
            if (current is PSMClass)
            {
                PSMClass old = current as PSMClass;
                PSMClass psmClass = ClassMap[old].DerivePSMClass();
                NewComponent = psmClass;
                psmClass.Name = old.Name;
                psmClass.ElementName = old.ElementName;

                ViewHelper v = new PSMElementViewHelper(NewPSMDiagram) { X = 0, Y = 0, Height = double.NaN, Width = double.NaN };
                NewPSMDiagram.AddModelElement(psmClass, v);
                psmClass.Diagram = NewPSMDiagram;

                //Attributes
                foreach (PSMAttribute A in old.PSMAttributes)
                {
                    PSMAttribute At = psmClass.AddAttribute() as PSMAttribute;
                    At.Name = A.Alias;
                    At.Lower = A.Lower;
                    At.Type = A.Type;
                    At.Upper = A.Upper;
                    At.Default = A.Default;
                    At.Alias = A.Alias;
                    if (AttrMap.ContainsKey(A))
                    {
                        At.RepresentedAttribute = AttrMap[A];
                        AttrMap[A].DerivedPSMAttributes.Add(At);
                    }
                }

                if (oldParentAssociation == null)
                {
                    NewPSMDiagram.Roots.Add(psmClass);
                }
                else
                {
                    PSMAssociation PSMAssoc = (PSMAssociation)parent.AddComponent(PSMAssociationFactory.Instance);
                    PSMAssoc.Child = psmClass;
                    PSMAssoc.Upper = oldParentAssociation.Upper;
                    PSMAssoc.Lower = oldParentAssociation.Lower;

                    NestingJoin N = PSMAssoc.AddNestingJoin(ClassMap[old]);
                    for (int i = 0; i < AssocMap[old].Count; i++)
                    {
                        N.Parent.AddStep(AssocMap[old][i].Start, AssocMap[old][i].End, AssocMap[old][i].Association);
                    }

                    NewPSMDiagram.AddModelElement(PSMAssoc, new PSMAssociationViewHelper(NewPSMDiagram));
                    PSMAssoc.Diagram = NewPSMDiagram;

                    foreach (Comment C in oldParentAssociation.Comments)
                    {
                        Comment NewC = PSMAssoc.AddComment(NameSuggestor<Comment>.SuggestUniqueName(current.Comments, "Comment", comment => comment.Body));
                        NewC.Body = C.Body;
                        NewPSMDiagram.AddModelElement(NewC, new CommentViewHelper(NewPSMDiagram));
                    }
                }

                foreach (PSMSubordinateComponent Sub in old.Components)
                    RegeneratePSM(Sub, psmClass);
            }
            else if (current is PSMContentChoice)
            {
                PSMContentChoice old = current as PSMContentChoice;
                PSMContentChoice psmChoice = (PSMContentChoice)parent.AddComponent(PSMContentChoiceFactory.Instance);
                NewPSMDiagram.AddModelElement(psmChoice, new PSMElementViewHelper(NewPSMDiagram));
                NewComponent = psmChoice;

                foreach (PSMSubordinateComponent Sub in current.Components)
                    RegeneratePSM(Sub, psmChoice);
            }
            else if (current is PSMContentContainer)
            {
                PSMContentContainer old = current as PSMContentContainer;

                PSMContentContainer psmContainer = (PSMContentContainer)parent.AddComponent(PSMContentContainerFactory.Instance);
                psmContainer.Name = old.ElementLabel;
                NewPSMDiagram.AddModelElement(psmContainer, new PSMElementViewHelper(NewPSMDiagram));
                NewComponent = psmContainer;

                foreach (PSMSubordinateComponent Sub in current.Components)
                    RegeneratePSM(Sub, psmContainer);
            }
            else
            {
                NewComponent = null;
            }

            if (NewComponent != null) foreach (Comment C in current.Comments)
            {
                Comment NewC = NewComponent.AddComment(NameSuggestor<Comment>.SuggestUniqueName(current.Comments, "Comment", comment => comment.Body));
                NewC.Body = C.Body;
                NewPSMDiagram.AddModelElement(NewC, new CommentViewHelper(NewPSMDiagram));
            }
        }

        #region Statistical output

        string[] DM_Description = { "Average", "Minimum", "Maximum" };

        char CSVsep = ',';

        class ClassFinalStatistic
        {
            public static int Stats = 7;
            public static bool[] StatsEnabled = { false, true, true, false, false, false, false };
            public double GP, LP, GPI, LPI, GPL, LPL, Stat;
            public double PreMapT;
            public int PreMappedClasses;
            public static string[] Description = { "Old statistic", "Global precision", "Local precision", "Global precision (inner)", "Local precision (inner)", "Global prescision (leaf)", "Local precision (leaf)" };
            public double this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return Stat;
                        case 1: return GP;
                        case 2: return LP;
                        case 3: return GPI;
                        case 4: return LPI;
                        case 5: return GPL;
                        case 6: return LPL;
                        default: throw new IndexOutOfRangeException();
                    }
                }
            }
        }
        ClassFinalStatistic[, , ,] ClassFinalStatistics;

        struct CandidateCombination
        {
            public bool Direct_Children, Subtree_leaves, Mapped_ancestors, Mapped_parent, Previous_siblings, Following_siblings;
            public string Description;
            public double PreMappingThreshold;
        }

        CandidateCombination[] Combs = 
        {   new CandidateCombination() { Description= "Children", Direct_Children = true, Mapped_ancestors = false, Previous_siblings = false, Subtree_leaves = false, Following_siblings = false, Mapped_parent = false, PreMappingThreshold = 1},
            new CandidateCombination() { Description= "Children + Prev. Siblings", Direct_Children = true, Mapped_ancestors = false, Previous_siblings = true, Subtree_leaves = false, Following_siblings = false, Mapped_parent = false, PreMappingThreshold = 1},
            new CandidateCombination() { Description= "Children + Parent + Follow. Siblings, 0.5", Direct_Children = true, Mapped_ancestors = false, Previous_siblings = false, Subtree_leaves = false, Following_siblings = true, Mapped_parent = true, PreMappingThreshold = 0.5},
            new CandidateCombination() { Description= "Children + Parent + Follow. Siblings, 0.75", Direct_Children = true, Mapped_ancestors = false, Previous_siblings = false, Subtree_leaves = false, Following_siblings = true, Mapped_parent = true, PreMappingThreshold = 0.75},
            new CandidateCombination() { Description= "Children + Prev. Siblings + Parent + Follow. Siblings, 0.35", Direct_Children = true, Mapped_ancestors = false, Previous_siblings = true, Subtree_leaves = false, Following_siblings = true, Mapped_parent = true, PreMappingThreshold = 0.35},
            new CandidateCombination() { Description= "Children + Prev. Siblings + Parent + Follow. Siblings, 0.5", Direct_Children = true, Mapped_ancestors = false, Previous_siblings = true, Subtree_leaves = false, Following_siblings = true, Mapped_parent = true, PreMappingThreshold = 0.5},
            new CandidateCombination() { Description= "Children + Prev. Siblings + Parent + Follow. Siblings, 0.75", Direct_Children = true, Mapped_ancestors = false, Previous_siblings = true, Subtree_leaves = false, Following_siblings = true, Mapped_parent = true, PreMappingThreshold = 0.75}
        };

        void Stat_SetDM(int DM)
        {
            switch (DM)
            {
                case 0:
                    ComputeClassAdjustments = ComputeClassAdjustments_Avg;
                    break;
                case 1:
                    ComputeClassAdjustments = ComputeClassAdjustments_Min;
                    break;
                case 2:
                    ComputeClassAdjustments = ComputeClassAdjustments_Max;
                    break;
            }
        }

        void Stat_SetCandidate(int Comb)
        {
            SSC_Direct_Children = Combs[Comb].Direct_Children;
            SSC_Mapped_Ancestors = Combs[Comb].Mapped_ancestors;
            SSC_Previous_Siblings = Combs[Comb].Previous_siblings;
            SSC_Subtree_Leaves = Combs[Comb].Subtree_leaves;
            SSC_Following_Siblings = Combs[Comb].Following_siblings;
            SSC_Mapped_Parent = Combs[Comb].Mapped_parent;
            if (Combs[Comb].PreMappingThreshold == 1) PreMapClasses = false;
            else
            {
                PreMapClasses = true;
                ClassPreMapThreshold = Combs[Comb].PreMappingThreshold;
            }
        }

        void StatRun()
        {
            if (Stat && !cancelled)
            {
                StatFirstRun = false;
                W.btnStart.IsEnabled = false;
                W.btnClose.IsEnabled = false;
                for (int SSim = 0; SSim <= 10; SSim++)
                {
                    for (int ASSim = 0; ASSim <= 10; ASSim++)
                    {
                        ClassSimWeightOfString = (double)SSim / 10;
                        ClassSimWeightOfStringAndAttr = (double)ASSim / 10;
                        AttrSimWeightOfString = 1;
                        attrSimWeightOfStringAndType = 0.5;
                        W.sldAttrStringAndTypeSim.Value = AttrSimWeightOfStringAndType;
                        W.sldAttrStringSim.Value = AttrSimWeightOfString;
                        W.sldClassStringAndAttrSim.Value = ClassSimWeightOfStringAndAttr;
                        W.sldClassStringSim.Value = ClassSimWeightOfString;
                        //for (int DM = 0; DM < 3; DM++)
                        //{
                        //    Stat_SetDM(DM);
                            for (int CComb = 0; CComb < Combs.Length; CComb++)
                            {
                                Stat_SetCandidate(CComb);

                                Algorithm();
                                ClassFinalStatistics[SSim, ASSim, 0/*DM*/, CComb] = new ClassFinalStatistic()
                                {
                                    Stat = stat,
                                    GP = GP,
                                    LP = LP,
                                    GPI = GPI,
                                    LPI = LPI,
                                    GPL = GPL,
                                    LPL = LPL,
                                    PreMapT = ClassPreMapThreshold,
                                    PreMappedClasses = ClassPreMap.Count
                                };
                            }
                        //}
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    }
                }

                //Write stats to CSV file

                SaveFileDialog D = new SaveFileDialog();
                D.Title = "Save statistics";
                D.FileName = diagramController.Diagram.Caption + ".csv";
                D.Filter = "Comma Separated Values (*.csv)|*.csv";
                D.CheckPathExists = true;
                D.ValidateNames = true;
                if (D.ShowDialog() == true)
                {
                    StreamWriter Wr;
                    try
                    {
                        Wr = new StreamWriter(D.FileName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }

                    //for (int DM = 0; DM < 3; DM++)
                    for (int DM = 0; DM <= 0; DM++)
                    {
                        for (int Comb = 0; Comb < Combs.Length; Comb++)
                        {
                            for (int StatNum = 0; StatNum < ClassFinalStatistic.Stats; StatNum++)
                            {
                                if (ClassFinalStatistic.StatsEnabled[StatNum])
                                {
                                    //Wr.WriteLine(DM_Description[DM]);
                                    Wr.WriteLine(Combs[Comb].Description);
                                    Wr.WriteLine(ComputeClassAdjustments.Method.Name);
                                    Wr.WriteLine("Sibling Adjustment: " + SiblingAdjustment);
                                    Wr.WriteLine(ClassFinalStatistic.Description[StatNum]);
                                    for (int SSim = -1; SSim <= 10; SSim++)
                                    {
                                        for (int ASSim = -1; ASSim <= 10; ASSim++)
                                        {
                                            if ((SSim == -1) && (ASSim == -1)) Wr.Write("1. V | 2. ->" + CSVsep);
                                            else if (SSim == -1) Wr.Write(((double)ASSim / 10).ToString() + CSVsep);
                                            else if (ASSim == -1) Wr.Write(((double)SSim / 10).ToString() + CSVsep);
                                            else Wr.Write(ClassFinalStatistics[SSim, ASSim, DM, Comb][StatNum].ToString() + CSVsep);
                                        }

                                        Wr.Write(CSVsep);
                                        //PreMapping
                                        if (SSim == -1) Wr.Write("Premapped #" + CSVsep);
                                        else Wr.Write(ClassFinalStatistics[SSim, 0, DM, Comb].PreMappedClasses.ToString() + CSVsep);

                                        Wr.WriteLine();
                                    }
                                    Wr.WriteLine();
                                }
                            }
                        }
                    }

                    Wr.WriteLine("Class Mappings:");
                    foreach (KeyValuePair<PSMClass, PIMClass> P in ClassMap)
                    {
                        Wr.WriteLine("{0} => {1}", P.Key.Name + (P.Key.HasElementLabel ? " (" + P.Key.ElementName + ')' : ""), P.Value.Name);
                    }
                    Wr.Close();
                }
                W.btnStart.IsEnabled = true;
                W.btnClose.IsEnabled = true;
            }
        }

        #endregion
    }
}
