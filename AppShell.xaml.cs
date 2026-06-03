namespace TasteHub
{
    /// <summary>
    /// Represents the central application shell.
    /// Defines the main visual hierarchy, navigation structure (such as tab bars), 
    /// and global routing mechanisms for the application.
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppShell"/> class.
        /// Sets up UI components and registers dynamic navigational routes.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            // Register routing definitions for pages that are navigated to programmatically (via URI).
            // This approach decouples navigation logic from the visual hierarchy, following best practices.
            Routing.RegisterRoute("DetailPage", typeof(Views.DetailPage));

            // Reusability Evidence: Reusing the same AddEditPage view for both creating new items and editing existing ones.
            Routing.RegisterRoute("AddEditPage", typeof(Views.AddEditPage));
            Routing.RegisterRoute("EditRecipePage", typeof(Views.AddEditPage));

            Routing.RegisterRoute("InteractivePage", typeof(Views.InteractivePage));
        }
    }
}