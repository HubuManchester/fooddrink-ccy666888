namespace TasteHub
{
    /// <summary>
    /// App shell defining the navigation structure with tab bar
    /// and registered routes for detail pages
    /// </summary>
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for pages that are navigated to programmatically
            Routing.RegisterRoute("DetailPage", typeof(Views.DetailPage));
            Routing.RegisterRoute("AddEditPage", typeof(Views.AddEditPage));
            Routing.RegisterRoute("InteractivePage", typeof(Views.InteractivePage));
        }
    }
}