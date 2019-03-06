using System;
using System.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace LRWRA
{
    internal class MakeLayersButtonPalette_sewersbutton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    // Check for "Manholes" and "Sewer Lines" layers in map.
                    var mapView = MapView.Active.Map;
                    var mhExists = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(m => m.Name == "Manholes");
                    var sewerExists = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(s => s.Name == "Sewer Lines");
                    if (mhExists == false && sewerExists == false)
                    {
                        MessageBox.Show("Manholes & Sewers are missing from map.", "Warning");
                    }
                    else if (mhExists == false && sewerExists)
                    {
                        MessageBox.Show("Sewer Lines layer is present. \n\nManholes layer is missing from map.", "Warning");
                    }
                    else if (mhExists && sewerExists == false)
                    {
                        MessageBox.Show("Manholes layer is present. \n\nSewers layer is missing from map.", "Warning");
                    }

                    else
                    {
                        SysModule.MakeSewersLayers(mapView);
                    }
                }

                catch (Exception ex)
                {
                    SysModule.LogError(ex.Message, ex.StackTrace);

                    string caption = "Create selection layer(s) failed!";
                    string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                        "If problem persist, contact your local GIS nerd.";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }
            });

        }
    }

    internal class MakeLayersButtonPalette_manholesbutton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    // Check for "Manholes" layer in map.
                    var mapView = MapView.Active.Map;

                    var linesExists = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(s => s.Name == "Manholes");

                    if (linesExists)
                    {
                        SysModule.MakeManholesLayer(mapView);
                    }

                    else
                    {
                        MessageBox.Show("There is no layer named 'Manholes' in map. " +
                            "\n\nIf a manholes layer is present, make sure the layer is named 'Manholes'. " +
                            "This tool will not work unless the layer is spelled exactly like above.", "Missing Layer(s)");
                    }
                }

                catch (Exception ex)
                {
                    SysModule.LogError(ex.Message, ex.StackTrace);

                    string caption = "Create selection layer(s) failed!";
                    string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                        "If problem persist, contact your local GIS nerd.";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }
            });
        }
    }

    internal class MakeLayersButtonPalette_linesbutton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    // Check for "Sewer Lines" layer in map.
                    var mapView = MapView.Active.Map;

                    var linesExists = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(s => s.Name == "Sewer Lines");
                    if (linesExists)
                    {
                        SysModule.MakeLinesLayer(mapView);
                    }

                    else
                    {
                        MessageBox.Show("There is no layer named 'Sewer Lines' in map. " +
                            "\n\nIf a sewer lines layer is present, make sure the layer is named 'Sewer Lines'. " +
                            "This tool will not work unless the layer is spelled exactly like above.", "Missing Layer(s)");
                    }
                }

                catch (Exception ex)
                {
                    SysModule.LogError(ex.Message, ex.StackTrace);

                    string caption = "Failed to Create Selection Layer(s)!";
                    string message = "Process failed. \n\nSave and restart ArcGIS Pro and try process again.\n\n" +
                        "If problem persist, contact your local GIS nerd.";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }
            });
        }
    }

}
