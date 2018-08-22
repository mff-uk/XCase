using System.Collections.Generic;
using XCase.Controller;
using XCase.Model;

namespace XCase.Gui
{
    /// <summary>
    /// This class was created to minimze code modifications after moving the Project class
    /// from Gui to Model project. It extends the Project class with some new methods
    /// concerning the ModelController association.
    /// </summary>
    public static class ProjectExtensions
    {
        /// <summary>
        /// Registers a new model controller with the project.
        /// </summary>
        /// <param name="project">References the current project</param>
        /// <param name="modelController">References the model controller to be associated with</param>
        public static void RegisterModelController(this Project project, ModelController modelController)
        {
            controllers[project] = modelController;
        }

        /// <summary>
        /// Creates a new model controller for the project.
        /// </summary>
        /// <param name="project">References the current project</param>
        public static void CreateModelController(this Project project)
        {
            ModelController controller = new ModelController(project.Schema.Model, project);
            controllers[project] = controller;
        }

        /// <summary>
        /// Unregisters the current model controller from the project.
        /// </summary>
        /// <param name="project">References the current project</param>
        public static void UnregisterModelController(this Project project)
        {
            controllers[project] = null;
        }

        /// <summary>
        /// Removes the project to model controller registration.
        /// </summary>
        /// <param name="project">References the current project</param>
        public static void RemoveControllerRegistration(this Project project)
        {
            controllers.Remove(project);
        }

        /// <summary>
        /// Gets the model controller associated with the project.
        /// </summary>
        /// <param name="project">References the current project</param>
        /// <returns>Reference to the associated model controller</returns>
        public static ModelController GetModelController(this Project project)
        {
            return controllers[project];
        }

        /// <summary>
        /// Dictionary translating a reference to a project to a reference to its model controller.
        /// </summary>
        private static Dictionary<Project, ModelController> controllers = new Dictionary<Project, ModelController>();
    }
}
