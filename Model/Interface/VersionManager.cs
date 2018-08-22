using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using XCase.Model.Implementation;

namespace XCase.Model
{
	public class VersionManager: IVersionManagerImpl, IEnumerable
	{
		#region internal infrastructure for keeping versions
		
		private readonly Hashtable versionTable = new Hashtable();

		private void addElementVersion(IVersionedElement firstVersion, Version version, IVersionedElement newVersion)
		{
            if (firstVersion.Version == version)
                throw new ArgumentException();
            if (firstVersion.GetType() != newVersion.GetType())
                throw new ArgumentException("Only constructs of the same type can be connected by version links. ");
			ComposedKey<IVersionedElement, Version> key = new ComposedKey<IVersionedElement, Version>(firstVersion, version);
			versionTable[key] = newVersion;
		}

        private void removeElementVersion(IVersionedElement versionedElement)
        {
            List<ComposedKey<IVersionedElement, Version>> keysToDelete = new List<ComposedKey<IVersionedElement,Version>>();
            Dictionary<ComposedKey<IVersionedElement, Version>, IVersionedElement> keysToAdd = new Dictionary<ComposedKey<IVersionedElement, Version>, IVersionedElement>();
            foreach (DictionaryEntry dictionaryEntry in versionTable)
            {
                ComposedKey<IVersionedElement, Version> key = (ComposedKey<IVersionedElement, Version>)dictionaryEntry.Key;
                IVersionedElement value = (IVersionedElement) dictionaryEntry.Value;

                // find all entries in version table dealing with versionedElement
                if (key.First.FirstVersion == versionedElement.FirstVersion)
                {
                    if (value.IsFirstVersion)
                    {
                        keysToDelete.Add(key);
                    }
                    else
                    {
                        // not deleting first version 
                        keysToDelete.Add(key);
                        Version ver = key.Second;
                        IVersionedElement fv = value.GetInVersion(ver.CreatedFrom);
                        if (fv != versionedElement.FirstVersion)
                            keysToAdd.Add(new ComposedKey<IVersionedElement, Version>(fv, ver), value);    
                    }

                }
            }

            foreach (ComposedKey<IVersionedElement, Version> key in keysToDelete)
            {
                if (!versionTable.ContainsKey(key))
                {

                }

                versionTable.Remove(key);
            }

            foreach (var de in keysToAdd)
            {
                versionTable.Add(de.Key, de.Value);
            }
        }

		internal IVersionedElement LookupElementVersion(IVersionedElement firstVersion, Version version)
		{
			ComposedKey<IVersionedElement, Version> key = new ComposedKey<IVersionedElement, Version>(firstVersion, version);
            if (versionTable.ContainsKey(key))
			    return (IVersionedElement)versionTable[key];  
            else
                return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceBranch"></param>
		/// <param name="newBranch"></param>
		/// <param name="version"></param>
		/// <param name="initialBranch">set to true when element is branched for the first time</param>
		/// <param name="firstVersion"></param>
		public void RegisterBranch(IVersionedElement sourceBranch, IVersionedElement newBranch, Version version, bool initialBranch, Version firstVersion)
		{
			if (initialBranch)
			{
				// source branch is first branch.. 
				SetAsFirstVersion(sourceBranch, firstVersion);
			}

			((_ImplVersionedElement)newBranch).Version = version;
			((_ImplVersionedElement)newBranch).FirstVersion = sourceBranch.FirstVersion;
			((_ImplVersionedElement)newBranch).VersionManager = this;

			addElementVersion(newBranch.FirstVersion, newBranch.Version, newBranch);
		}

		internal void SetAsFirstVersion(IVersionedElement sourceBranch, Version firstVersion)
		{
			((_ImplVersionedElement)sourceBranch).Version = firstVersion;
			((_ImplVersionedElement)sourceBranch).FirstVersion = sourceBranch;
			((_ImplVersionedElement)sourceBranch).VersionManager = this;
            firstVersion.ElementsCreatedInVersion.Add(sourceBranch);
		}

        /// <summary>
        /// Marks the version of <paramref name="versionedElement"/> as its first version
        /// and all its branches as branches of it. 
        /// </summary>
        /// <param name="versionedElement">versioned element</param>
        public void MakeIndependentOfOlderVersions(IVersionedElement versionedElement)
        {
            List<Version> branchedVersions = versionedElement.Version.BranchedVersions.ToList();
            foreach (Version branchedVersion in branchedVersions)
            {
                // I try to look up the element in all the derived version
                IVersionedElement derivedElement = versionedElement.GetInVersion(branchedVersion);
                if (derivedElement != null)
                {
                    // this derivedElement is now a first version
                    removeElementVersion(derivedElement);
                    ((_ImplVersionedElement)derivedElement).FirstVersion = versionedElement;
                    addElementVersion(versionedElement, branchedVersion, derivedElement);
                    
                    //SetAsFirstVersion(derivedElement, versionedElement.Version);
                }
            }

            removeElementVersion(versionedElement);
            SetAsFirstVersion(versionedElement, versionedElement.Version);
        }

	    private Dictionary<Version, IVersionedElement> SetThisAsFirstVersionForAllBranchedVersions(IVersionedElement versionedElement)
	    {
	        Dictionary<Version, IVersionedElement> affectedDerived = new Dictionary<Version, IVersionedElement>();
	        List<Version> branchedVersions = versionedElement.Version.BranchedVersions.ToList();
	        foreach (Version branchedVersion in branchedVersions)
	        {
	            // I try to look up the element in all the derived version
	            IVersionedElement derivedElement = versionedElement.GetInVersion(branchedVersion);
	            if (derivedElement != null)
	            {
	                affectedDerived[branchedVersion] = derivedElement;
	                // this derivedElement is now a first version
	                SetAsFirstVersion(derivedElement, branchedVersion);
	            }
	        }
	        return affectedDerived;
	    }


	    public Dictionary<Version, IVersionedElement> UnregisterBranch(IVersionedElement versionedElement)
        {
	        Dictionary<Version, IVersionedElement> affectedDerived =
	            SetThisAsFirstVersionForAllBranchedVersions(versionedElement);

            removeElementVersion(versionedElement);
            Debug.Assert(!versionTable.ContainsKey(new ComposedKey<IVersionedElement, Version>(versionedElement.FirstVersion, versionedElement.Version)));
            return affectedDerived;
        }

	    public void ReRegisterBranch(IVersionedElement versionedElement, Dictionary<Version, IVersionedElement> affectedDerived)
        {
            List<Version> branchedVersions = versionedElement.Version.BranchedVersions.ToList();
            foreach (Version branchedVersion in branchedVersions)
            {
                // I try to look up the element in all the derived version
                if (affectedDerived.ContainsKey(branchedVersion))
                {
                    IVersionedElement derivedElement = affectedDerived[branchedVersion];
                    Debug.Assert(derivedElement.IsFirstVersion);
                
                    RegisterBranch(versionedElement.FirstVersion, derivedElement, branchedVersion, versionedElement.IsFirstVersion, versionedElement.FirstVersion.Version);

                    Debug.Assert(derivedElement.FirstVersion == versionedElement);

                }
            }
        }

		#endregion
        
        public void DeleteVersion(Version removedVersion)
	    {
	        Debug.Assert(Versions.Contains(removedVersion));
	        versions.Remove(removedVersion);
	        versionedProjects.Remove(removedVersion);
            Versions.InvokeCollectionChanged();

	        if (removedVersion.CreatedFrom != null)
	        {
	            Version previous = removedVersion.CreatedFrom;

	            List<Version> branchedVersions = removedVersion.BranchedVersions.ToList();
	            foreach (Version branchedVersion in branchedVersions)
	            {
	                branchedVersion.CreatedFrom = previous;

                    // there are some elements created in deleted removedVersion
	                foreach (IVersionedElement createdInVersion in removedVersion.ElementsCreatedInVersion)
	                {
                        // I try to look up the element in all the derived removedVersion
	                    IVersionedElement derivedElement = this.LookupElementVersion(createdInVersion, branchedVersion);
	                    if (derivedElement != null)
	                    {
                            // this derivedElement is now a first removedVersion
                            SetAsFirstVersion(derivedElement, branchedVersion);
	                    }
	                }
	            }

	            previous.BranchedVersions.Remove(removedVersion);
	        }

            List<ComposedKey<IVersionedElement, Version>> toRemove = new List<ComposedKey<IVersionedElement, Version>>();
            foreach (DictionaryEntry entry in versionTable)
            {

                ComposedKey<IVersionedElement, Version> key = (ComposedKey<IVersionedElement, Version>) entry.Key;
                IVersionedElement originalElement = key.First;
                Version derivedVersion = key.Second;
                IVersionedElement derivedElement = (IVersionedElement) entry.Value;

                if (derivedVersion == removedVersion)
                {
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
            {
                versionTable.Remove(key);
            }
	    }

		private readonly Dictionary<Version, Project> versionedProjects = new Dictionary<Version, Project>();

		private ReadOnlyDictionary<Version, Project> versionedProjectsRO;

		public IReadOnlyDictionary<Version, Project> VersionedProjects
		{
			get
			{
				if (versionedProjectsRO == null)
				{
					versionedProjectsRO = versionedProjects.AsReadOnly();
				}
				return versionedProjectsRO;
			}
		}

		private readonly List<Version> versions = new List<Version>();

		private NotifyingReadOnlyCollection<Version> _versionsRO;

		public NotifyingReadOnlyCollection<Version> Versions 
		{
			get
			{
				if (_versionsRO == null)
				{
					_versionsRO = new NotifyingReadOnlyCollection<Version>(versions);
				}
				return _versionsRO;
			}
		}

		#region Implementation of IVersionManagerImpl

		public void AddVersion(Version version)
		{
			versions.Add(version);
		}

		public void AddVersionedProject(Version version, Project project)
		{
			versionedProjects[version] = project;
		}

		#endregion

		public Project LatestVersion
		{
			get
			{
				if (VersionedProjects.Count > 0)
					return VersionedProjects[Versions.Last()];
				else return null;
			}
		}

		public Project FirstVersion
		{
			get
			{
				if (VersionedProjects.Count > 0) 
					return VersionedProjects[Versions.First()];
				else return null;
			}
		}

		public string FilePath { get; set; }

		public Project BranchProject(Project project, IEnumerable<Type> PIMElementsOrder, bool initialBranch)
		{
			Project branchProject = new Project(project.Caption);
			Model sourceModel = project.Schema.Model;
			Model targetModel = branchProject.Schema.Model;
			
			branchProject.FilePath = project.FilePath;
			branchProject.Schema.XMLNamespace = project.Schema.XMLNamespace;
			// TODO: remove these names
			project.Schema.Name = "OLD SCHEMA";
			branchProject.Schema.Name = "NEW SCHEMA";

			targetModel.Name = sourceModel.Name;

            ElementCopiesMap createdCopies = new ElementCopiesMap();
			Dictionary<Diagram, Diagram> createdDiagrams = new Dictionary<Diagram, Diagram>();
			createdCopies[sourceModel] = targetModel;
			List<Package> packages = new List<Package> { sourceModel };

			#region packages structure

			{
				int index = 0;
				while (packages.Count > index)
				{
					Package sourcePackage = packages[index];
					if (sourcePackage != sourceModel)
					{
						Package copyPackage = (Package)sourcePackage.CreateCopy(targetModel, createdCopies);
						createdCopies[sourcePackage] = copyPackage;
					}

					packages.AddRange(sourcePackage.NestedPackages);
					index++;
				}
			}

			#endregion

			#region primitive types

            branchProject.TemplateIdTable = new Serialization.SerializatorIdTable();
			foreach (SimpleDataType primitiveType in project.Schema.PrimitiveTypes)
			{
				SimpleDataType copyPrimitiveType = (SimpleDataType)primitiveType.CreateCopy(targetModel, createdCopies);
				createdCopies[primitiveType] = copyPrimitiveType;
			    branchProject.TemplateIdTable[copyPrimitiveType] = project.TemplateIdTable[primitiveType];
			}

			#endregion

			#region profiles

			foreach (Profile profile in project.Schema.Profiles)
			{
				Profile copyProfile = branchProject.Schema.AddProfile();
				profile.FillCopy(copyProfile, targetModel, createdCopies);
                if (profile.Name == "XSem")
                {
                    createdCopies.SubElements.Clear();
                    createdCopies.Remove(profile);
                }
			}

			#endregion

			#region classes (not association classes!)

			List<AssociationClass> associationClasses = new List<AssociationClass>();

			foreach (Package package in packages)
			{
				foreach (PIMClass pimClass in package.Classes)
				{
					if (pimClass is AssociationClass)
					{
						associationClasses.Add((AssociationClass)pimClass);
					}
					else
					{
						Element copyClass = pimClass.CreateCopy(targetModel, createdCopies);
						createdCopies[pimClass] = copyClass;
					}
				}
			}

			#endregion

            #region pim comments 

            // in PIM diagrams
            foreach (PIMDiagram diagram in project.PIMDiagrams)
            {
                foreach (Comment comment in diagram.DiagramElements.Keys.OfType<Comment>())
                {
                    Element copyComment = comment.CreateCopy(targetModel, createdCopies);
                    createdCopies[comment] = copyComment;
                }
            }

            // in model
            foreach (Comment comment in project.Schema.Model.Comments)
		    {
                Element copyComment = comment.CreateCopy(targetModel, createdCopies);
                createdCopies[comment] = copyComment;
		    }

            #endregion

			#region AssociationClasses

			{
				// try to copy association classes, if not all ends are ready yet, skip the class for now.
				bool canContinue = associationClasses.Count > 0;
				int index = 0;
				while (index <= associationClasses.Count - 1)
				{
					AssociationClass associationClass = associationClasses[index];
					if (associationClass.Ends.All(end => createdCopies.ContainsKey(end.Class)))
					{
						Element copyAssociationClass = associationClass.CreateCopy(targetModel, createdCopies);
						createdCopies[associationClass] = copyAssociationClass;
						associationClasses.RemoveAt(index);
						canContinue = true;
					}
					else
					{
						index++;
					}
					if (index == associationClasses.Count && canContinue)
					{
						index = 0;
						canContinue = false;
					}
				}
				if (associationClasses.Count != 0)
				{
					throw new InvalidOperationException("Failed to branch association classes. ");
				}
			}

			#endregion

			#region PIM generalizations

			foreach (Generalization generalization in sourceModel.Generalizations)
			{
				if (!(generalization.General is PSMClass))
				{
					Element copyGeneralization = generalization.CreateCopy(targetModel, createdCopies);

					createdCopies[generalization] = copyGeneralization;
				}
			}


			#endregion

			#region associations (without association classes)

			foreach (Association association in sourceModel.Associations.Where(association => !(association is AssociationClass)))
			{
				Element copyAssociation = association.CreateCopy(targetModel, createdCopies);

				createdCopies[association] = copyAssociation;
			}

			#endregion

			#region create PIM diagrams

			{
				foreach (PIMDiagram pimDiagram in project.PIMDiagrams)
				{
					PIMDiagram pimDiagramCopy = (PIMDiagram)pimDiagram.Clone();

					Dictionary<Element, ViewHelper> viewHelperCopies = new Dictionary<Element, ViewHelper>();

					/* Elements in PIM diagram are loaded in the order of their LoadPriority in registration set */
					foreach (Type ModelElementType in PIMElementsOrder)
					{
						foreach (KeyValuePair<Element, ViewHelper> pair in pimDiagram.DiagramElements)
						{
							Element element = pair.Key;
							ViewHelper viewHelper = pair.Value;

							if (!viewHelperCopies.ContainsKey(element) && ModelElementType.IsInstanceOfType(element))
							{
								ViewHelper copiedViewHelper = viewHelper.CreateCopy(pimDiagramCopy, createdCopies);
								viewHelperCopies.Add(element, copiedViewHelper);

								pimDiagramCopy.AddModelElement(createdCopies[element], copiedViewHelper);
							}
						}
					}

					branchProject.AddPIMDiagram(pimDiagramCopy);
					createdDiagrams[pimDiagram] = pimDiagramCopy;
				}
			}

			#endregion

			#region create PSM diagrams

			{
				foreach (PSMDiagram psmDiagram in project.PSMDiagrams)
				{
					PSMDiagram psmDiagramCopy = (PSMDiagram)psmDiagram.Clone();

					IList<Element> ordered;

					// order 
					PSMTree.ReturnElementsInPSMOrder((psmDiagram).Roots.Cast<Element>(), out ordered, true);

					// clone PSM elements
					foreach (Element element in ordered)
					{
						Element copy = element.CreateCopy(targetModel, createdCopies);
						createdCopies[element] = copy;

                        foreach (Comment comment in element.Comments)
                        {
                            Element copyComment = comment.CreateCopy(targetModel, createdCopies);
                            createdCopies[comment] = copyComment;
                        }
					}

                    // clone comments
				    foreach (Comment comment in psmDiagram.DiagramElements.Keys.OfType<Comment>())
				    {
                        Element copyComment = comment.CreateCopy(targetModel, createdCopies);
                        createdCopies[comment] = copyComment;
				    }

                    // clone references 
				    foreach (PSMDiagramReference psmDiagramReference in psmDiagram.DiagramReferences)
				    {
				        PSMDiagramReference copyReference = (PSMDiagramReference) psmDiagramReference.CreateCopy(targetModel, createdCopies);
				        copyReference.ReferencingDiagram = psmDiagramCopy;
				        createdCopies[psmDiagramReference] = copyReference;
				    }

					// representants must be handled separately after all copies are created 
					PSMTree.CopyRepresentantsRelations(createdCopies);

					// clone viewhelpers
					Dictionary<Element, ViewHelper> createdViewHelpers = new Dictionary<Element, ViewHelper>();

                    foreach (Element element in ordered)
                    {
                        ViewHelper viewHelper = psmDiagram.DiagramElements[element];
                        ViewHelper copiedViewHelper = viewHelper.CreateCopy(psmDiagramCopy, createdCopies);

                        createdViewHelpers.Add(element, copiedViewHelper);

                        foreach (Comment comment in element.Comments)
                        {
                            ViewHelper commentViewHelper = psmDiagram.DiagramElements[comment];
                            ViewHelper copiedCommentViewHelper = commentViewHelper.CreateCopy(psmDiagramCopy, createdCopies);
                            createdViewHelpers.Add(comment, copiedCommentViewHelper);
                        }
                    }

                    foreach (PSMDiagramReference psmDiagramReference in psmDiagram.DiagramReferences)
                    {
                        ViewHelper referenceViewHelper = psmDiagram.DiagramElements[psmDiagramReference];
                        ViewHelper copiedReferenceViewHelper = referenceViewHelper.CreateCopy(psmDiagramCopy, createdCopies);
                        createdViewHelpers.Add(psmDiagramReference, copiedReferenceViewHelper);
                    }

                    foreach (Comment comment in psmDiagram.DiagramElements.Keys.OfType<Comment>())
                    {
                        ViewHelper commentViewHelper = psmDiagram.DiagramElements[comment];
                        ViewHelper copiedCommentViewHelper = commentViewHelper.CreateCopy(psmDiagramCopy, createdCopies);
                        createdViewHelpers.Add(comment, copiedCommentViewHelper);
                    }

				    psmDiagram.FillCopy(psmDiagramCopy, targetModel, createdCopies, createdViewHelpers);

					branchProject.AddPSMDiagram(psmDiagramCopy);
					createdDiagrams[psmDiagram] = psmDiagramCopy;
				}

			    foreach (PSMDiagram psmDiagram in project.PSMDiagrams)
			    {
			        foreach (PSMDiagramReference psmDiagramReference in psmDiagram.DiagramReferences)
			        {
			            PSMDiagramReference copyReference = (PSMDiagramReference) createdCopies[psmDiagramReference];
                        copyReference.ReferencedDiagram = (PSMDiagram)createdDiagrams[psmDiagramReference.ReferencedDiagram];
			        }
			    }
			}

			#endregion

			#region comments

			// there may be some comments that are not part of any diagram and they are copied now. 

			foreach (KeyValuePair<Element, Element> kvp in createdCopies)
			{
				Element source = kvp.Key;

				foreach (Comment comment in source.Comments)
				{
					if (!createdCopies.ContainsKey(comment))
					{
						comment.CreateCopy(targetModel, createdCopies);
					}
				}
			}

			#endregion

			int next;

		    Version firstVersion; 
			if (initialBranch)
			{
				firstVersion = new Version { Number = 1 };
                versions.Add(firstVersion);
				versionedProjects[firstVersion] = project;
			    SetAsFirstVersion(project, firstVersion);
				next = 2;
			}
			else
			{
				next = versions.Max(version => version.Number) + 1;
			    firstVersion = project.FirstVersion.Version;
			}

			Version newVersion = new Version { Number = next, CreatedFrom = project.Version };
            versions.Add(newVersion);

            RegisterBranch(sourceModel, targetModel, newVersion, initialBranch, firstVersion);
            RegisterBranch(project, branchProject, newVersion, initialBranch, firstVersion);
            
            Versions.InvokeCollectionChanged();

            #region register element branches 

            foreach (KeyValuePair<Element, Element> keyValuePair in createdCopies)
			{
				IVersionedElement sourceVersion = keyValuePair.Key;
				IVersionedElement branchedVersion = keyValuePair.Value;

				RegisterBranch(sourceVersion, branchedVersion, newVersion, initialBranch, firstVersion);
			}

			foreach (KeyValuePair<Diagram, Diagram> keyValuePair in createdDiagrams)
			{
				IVersionedElement sourceVersion = keyValuePair.Key;
				IVersionedElement branchedVersion = keyValuePair.Value;

                RegisterBranch(sourceVersion, branchedVersion, newVersion, initialBranch, firstVersion);
			}

		    foreach (SubElementCopiesMap subElementCopiesMap in createdCopies.SubElements.Values)
		    {
		        foreach (KeyValuePair<Element, Element> keyValuePair in subElementCopiesMap)
		        {
                    IVersionedElement sourceVersion = keyValuePair.Key;
                    IVersionedElement branchedVersion = keyValuePair.Value;

                    RegisterBranch(sourceVersion, branchedVersion, newVersion, initialBranch, firstVersion);
		        }
            }

            #endregion 

            versionedProjects[newVersion] = branchProject; 

			return branchProject;
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public IEnumerator GetEnumerator()
		{
			return versionTable.GetEnumerator();
		}

		#endregion
	}

	internal interface IVersionManagerImpl
	{
		void AddVersion(Version version);
		void AddVersionedProject(Version version, Project project);
	}
}