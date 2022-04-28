using System;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.IO;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using System.Linq;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Collections.Generic;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing.Attributes;

namespace LRWRA
{
    internal class SysModule : Module
    {
        private static SysModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static SysModule Current
        {
            get
            {
                return _this ?? (_this = (SysModule)FrameworkApplication.FindModule("LRWRA_Module"));
            }
        }

        // Method used to log any exceptions thrown by my users. Log is written to a .txt file on our LAN.
        public static void LogError(string Message, string StackTrace)
        {
            TextWriter writer = File.AppendText(@"O:\SHARE\405 - INFORMATION SERVICES\Middle Earth\ArcGIS Pro\ExceptionLog.txt");
            try
            {
                writer.WriteLine("***********************************************");
                writer.WriteLine(Environment.MachineName);// gets PC name for user that causes the excepton,
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(Message);
                writer.WriteLine(StackTrace);
                writer.WriteLine();

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            finally
            {
                writer.Close();
            }

        }

        // Methods to create the selection layers.  
        public static void MakeSewersLayers(Map mapView)
        {
            try
            {
                var mh = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(s => s.Name == "Manholes");
                var lines = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(s => s.Name == "Sewer Lines");

                //Get the selected features from the map.
                var mhOIDList = mh.GetSelection().GetObjectIDs();// Gets a list of Object IDs
                var mhOID = mh.GetTable().GetDefinition().GetObjectIDField();// Gets the OBJECTID field name for the def query
                var linesOIDList = lines.GetSelection().GetObjectIDs();
                var linesOID = lines.GetTable().GetDefinition().GetObjectIDField();

                //Check to see if there are mmanhole or sewer line features selected in map.
                if (mhOIDList.Count() == 0 && linesOIDList.Count() == 0)
                {
                    MessageBox.Show("Manholes & Sewers contain no selected features.", "Warning");
                }

                else if (mhOIDList.Count() == 0 && linesOIDList.Count() > 0)
                {
                    MessageBox.Show("Manholes layer contains no selected features.", "Warning");
                }

                else if (mhOIDList.Count() > 0 && linesOIDList.Count() == 0)
                {
                    MessageBox.Show("Sewer Lines layer contains no selected features.", "Warning");
                }

                else
                {

                    // CREATE THE SEWER LINES SELECTION LAYER
                    // Create the defenition query
                    string linesDefQuery = $"{linesOID} in ({string.Join(",", linesOIDList)})";
                    string linesURL = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.SEWERMAN.SEWERS_VIEW";

                    // Create the Uri object create the feature layer.
                    Uri linesURI = new Uri(linesURL);
                    var linesSelLayer = LayerFactory.Instance.CreateFeatureLayer(linesURI, mapView, 0, "Sewer Lines SELECTION");

                    // Apply the definition query
                    linesSelLayer.SetDefinitionQuery(linesDefQuery);

                    // Create the line symbol renderer.
                    CIMLineSymbol lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(
                        ColorFactory.Instance.RedRGB,
                        3.0,
                        SimpleLineStyle.Solid
                        );
                    CIMSimpleRenderer lineRenderer = linesSelLayer.GetRenderer() as CIMSimpleRenderer;

                    // Renference the existing renderer
                    lineRenderer.Symbol = lineSymbol.MakeSymbolReference();
                    // Apply the new renderer
                    linesSelLayer.SetRenderer(lineRenderer);


                    // CREATE THE MANHOLES SELECTION LAYER
                    // Create the defenition query
                    string mhDefQuery = $"{mhOID} in ({string.Join(",", mhOIDList)})";
                    string mhURL = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.SEWERMAN.MANHOLES_VIEW";

                    // Create the Uri object create the feature layer.
                    Uri mhURI = new Uri(mhURL);
                    var mhSelLayer = LayerFactory.Instance.CreateFeatureLayer(mhURI, mapView, 0, "Manholes SELECTION");

                    // Apply the definition query
                    mhSelLayer.SetDefinitionQuery(mhDefQuery);

                    // Create the point symbol renderer.
                    CIMPointSymbol pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(
                        ColorFactory.Instance.GreenRGB,
                        8.0,
                        SimpleMarkerStyle.Circle
                        );
                    CIMSimpleRenderer pointRenderer = mhSelLayer.GetRenderer() as CIMSimpleRenderer;

                    // Renference the existing renderer
                    pointRenderer.Symbol = pointSymbol.MakeSymbolReference();
                    // Apply the new renderer
                    mhSelLayer.SetRenderer(pointRenderer);
                }
            }

            catch (Exception ex)
            {
                LogError(ex.Message, ex.StackTrace);

                string caption = "Error Occurred";
                string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);

            }
        }

        public static void MakeManholesLayer(Map mapView)
        {
            try
            {
                // Create the "Manholes"  layer object.
                var mh = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(s => s.Name == "Manholes");

                //Get the selected features from the map.
                var mhOIDList = mh.GetSelection().GetObjectIDs();// Gets a list of Object IDs
                var mhOID = mh.GetTable().GetDefinition().GetObjectIDField(); // Gets the OBJECTID field name for the def query

                // Check to see if there are manhole features selected in the map.
                if (mhOIDList.Count() == 0)
                {
                    MessageBox.Show("There are no manholes selected.");
                }

                else
                {
                    // Create the defenition query
                    string defQuery = $"{mhOID} in ({string.Join(",", mhOIDList)})";
                    string url = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.SEWERMAN.MANHOLES_VIEW";

                    // Create the Uri object create the feature layer.
                    Uri uri = new Uri(url);
                    var selectionLayer = LayerFactory.Instance.CreateFeatureLayer(uri, mapView, 0, "Manholes SELECTION");

                    // Apply the definition query
                    selectionLayer.SetDefinitionQuery(defQuery);

                    // Create the point symbol renderer.
                    CIMPointSymbol pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(
                        ColorFactory.Instance.GreenRGB,
                        8.0,
                        SimpleMarkerStyle.Circle
                        );
                    CIMSimpleRenderer renderer = selectionLayer.GetRenderer() as CIMSimpleRenderer;
                    // Reference the existing renderer
                    renderer.Symbol = pointSymbol.MakeSymbolReference();
                    // Apply new renderer
                    selectionLayer.SetRenderer(renderer);
                }
            }

            catch (Exception ex)
            {
                SysModule.LogError(ex.Message, ex.StackTrace);

                string caption = "Error Occurred";
                string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }

        }

        public static void MakeLinesLayer(Map mapView)
        {
            try
            {
                // Create the "Sewer Lines" object.
                var lines = mapView.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(s => s.Name == "Sewer Lines");

                //Get the selected features from the map.
                var linesOIDList = lines.GetSelection().GetObjectIDs();// Gets a list of Object IDs
                var linesOID = lines.GetTable().GetDefinition().GetObjectIDField();// Gets the OBJECTID field name for the def query

                // Check to see if there are sewer lines features selected inthe map.
                if (linesOIDList.Count() == 0)
                {
                    MessageBox.Show("There are no sewer lines selected.");
                }

                else
                {
                    //Create the defenition query
                    string defQuery = $"{linesOID} in ({string.Join(",", linesOIDList)})";
                    string url = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\GISVIEWER.SDE@SQL0.sde\SDE.SEWERMAN.SEWERS_VIEW";

                    // Create the Uri object create the feature layer.
                    Uri uri = new Uri(url);
                    var selectionLayer = LayerFactory.Instance.CreateFeatureLayer(uri, mapView, 0, "Sewer Lines SELECTION");

                    // Apply the definition query
                    selectionLayer.SetDefinitionQuery(defQuery);

                    // Create the line symbol renderer.
                    CIMLineSymbol lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(
                        ColorFactory.Instance.RedRGB,
                        3.0,
                        SimpleLineStyle.Solid
                        );
                    CIMSimpleRenderer renderer = selectionLayer.GetRenderer() as CIMSimpleRenderer;

                    // Reference the existing renderer
                    renderer.Symbol = lineSymbol.MakeSymbolReference();
                    // Apply the new renderer
                    selectionLayer.SetRenderer(renderer);
                }
            }

            catch (Exception ex)
            {
                SysModule.LogError(ex.Message, ex.StackTrace);

                string caption = "Error Occurred";
                string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }
        }

        public static void SetDisplayField(StandaloneTable saTable, string tableName, string fieldName)
        {
            var mapView = MapView.Active.Map;

            QueuedTask.Run(() =>
            {
                //Gets a list of the standalone tables in map with the name specified in the method arguments.
                IReadOnlyList<StandaloneTable> tables = mapView.FindStandaloneTables(tableName);
                foreach (var table in tables)
                {
                    //Gets the list of fields from the stand alone table.
                    var descriptions = table.GetFieldDescriptions();
                    foreach (var desc in descriptions)
                    {
                        if (desc.Name == fieldName) // If field name equals "COMPDTTM"
                        {
                            //Creates variable that's equal to "COMPDTTM"
                            string displayName = desc.Name;

                            // Get's the CIM definition from the StandaloneTable
                            var cimTableDefinition = table.GetDefinition() as CIMStandaloneTable;

                            // Set DisplayField property of the cimTableDefinition equall to displayName("COMPDTTM")
                            cimTableDefinition.DisplayField = displayName;

                            // USe the SetDefinition method to apply the modified table definition (cimTableDefninition)
                            saTable.SetDefinition(cimTableDefinition);

                            // Use to check the result when debugin in break point.
                            var result = table.GetDefinition().DisplayField;
                        }
                    }
                }
            });
        }

        public static void BuildDictionariesAsync(Dictionary<int, List<string>> arcNodeListDictionary, Dictionary<string, List<int>> nodeArcListDictionary)
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    // Used to free up process memory. Without this the trace tool will 'hang up' after going between trace up and trace down.
                    nodeArcListDictionary.Clear();
                    arcNodeListDictionary.Clear();

                    // Global vairables
                    var map = MapView.Active.Map;
                    var arcLayer = map.FindLayers("Sewer Lines").FirstOrDefault() as FeatureLayer;
                    var nodeLayer = map.FindLayers("Manholes").FirstOrDefault() as FeatureLayer;

                    var arcTableDef = arcLayer.GetTable().GetDefinition(); //table definition of featurelayer
                    var nodeTableDef = nodeLayer.GetTable().GetDefinition(); //table definition of featurelayer

                    // BUILD ARC AND NODE DICTIONARIES
                    // arc ObjectID-- > { UPS_MH, DWN_MH}  Only 2 VALUES for each KEY
                    // node MH Number -- >{ Arc OBjectID, Arc OBjectID, ...} Can have 1 or more VALUES for each KEY

                    // Get the indices for the fields
                    int objIDIdx = arcTableDef.FindField("ObjectID");
                    int nodeUpIdx = arcTableDef.FindField("UNITID");
                    int nodeDwnIdx = arcTableDef.FindField("UNITID2");

                    using (ArcGIS.Core.Data.RowCursor rowCursor = arcLayer.Search())
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                //List<string> unitIDValueList = new List<string>();
                                //List<int> objIDValueList = new List<int>();
                                var objIDVal = row.GetOriginalValue(objIDIdx);
                                var nodeUpVal = row.GetOriginalValue(nodeUpIdx);
                                var nodeDownVal = row.GetOriginalValue(nodeDwnIdx);

                                // Populate arcNodeListDictionary keys and values
                                if (arcNodeListDictionary.ContainsKey((int)objIDVal))
                                {
                                    //Do nothing
                                }
                                else
                                {
                                    arcNodeListDictionary.Add((int)objIDVal, new List<string>());
                                    arcNodeListDictionary[(int)objIDVal].Add((string)nodeUpVal);
                                    arcNodeListDictionary[(int)objIDVal].Add((string)nodeDownVal);
                                }

                                // Check of the nodeArcListDictionary contains nodeUpVal as KEY- Add nodeUpVal if FALSE
                                if (nodeArcListDictionary.ContainsKey((string)nodeUpVal))
                                {

                                    nodeArcListDictionary[(string)nodeUpVal].Add((int)objIDVal);

                                }
                                else
                                {
                                    nodeArcListDictionary.Add((string)nodeUpVal, new List<int>());
                                    nodeArcListDictionary[(string)nodeUpVal].Add((int)objIDVal);
                                }

                                // Check of the nodeArcListDictionary contains nodeDownVal as KEY- Add nodeDownVal if FALSE
                                if (nodeArcListDictionary.ContainsKey((string)nodeDownVal))
                                {
                                    //Do nothing
                                    nodeArcListDictionary[(string)nodeDownVal].Add((int)objIDVal);
                                }
                                else
                                {
                                    nodeArcListDictionary.Add((string)nodeDownVal, new List<int>());
                                    nodeArcListDictionary[(string)nodeDownVal].Add((int)objIDVal);
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    LogError(ex.Message, ex.StackTrace);

                    string caption = "BuildDictionariesAsync failed!";
                    string message = "Background process failed. Trace tools will NOT work. \n\nSave and restart ArcGIS Pro and try process again.\n" +
                        "If problem persist, contact your GIS Admin.";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }
 
            });
        }

        public static void SewersStatus()
        {
            try
            {


                var map = MapView.Active.Map;
                var mhExists = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(m => m.Name == "Manholes");
                var sewerExists = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(s => s.Name == "Sewer Lines");
                if (mhExists == false && sewerExists == false)
                {
                    MessageBox.Show("Manholes & Sewers are missing from map.", "Layer(s) Missing");
                }
                else if (mhExists == false && sewerExists)
                {
                    MessageBox.Show("Sewer Lines layer is present. \n\nManholes layer is missing from map.", "Layer(s) Missing");
                }
                else if (mhExists && sewerExists == false)
                {
                    MessageBox.Show("Manholes layer is present. \n\nSewers layer is missing from map.", "Layer(s) Missing");
                }

                else
                {
                    var layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                    foreach (var layer in layers)
                    {
                        if (layer.Name == "Manholes" || layer.Name == "Sewer Lines")
                        //if (layer.Name == "Manholes")
                        {
                            layer.SetSelectable(true);
                        }

                        else
                        {
                            layer.SetSelectable(false);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                SysModule.LogError(ex.Message, ex.StackTrace);

                string caption = "Error Occurred";
                string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }
        }

        public static void SewerLinesStatus()
        {
            try
            {
                var map = MapView.Active.Map;
                map.SetSelection(null);// Clears all current selections
                var mhExists = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(m => m.Name == "Manholes");
                var sewerExists = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(s => s.Name == "Sewer Lines");

                // Check for the SEWER LINES Layer and MANHOLES layers in the map.
                if (!sewerExists)
                {
                    MessageBox.Show("Sewer Lines are missing from map.\n 'Sewer Lines' " +
                        "layer must be named exactly as such, and be the only such named layer, for tool to work",
                        "Warning", "Missing Layer");
                }

                else
                {
                    //Make Sewer Lines the only selectabe layer in map.
                    var layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                    foreach (var layer in layers)
                    {

                        if (layer.Name == "Sewer Lines")
                        {
                            layer.SetSelectable(true);
                        }
                        else
                        {
                            layer.SetSelectable(false);
                        }
                    }
                }
            }

            catch (Exception)
            {
                string caption = "Error Occurred";
                string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }
        }

        //public static string GetPMCompkey()
        //{
        //    string compkey;


        //    try
        //    {
        //        //ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New);

        //        var map = MapView.Active.Map;
        //        var linesLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((m => m.Name == "Sewer Lines"));

        //        // Get the currently selected features in the map
        //        var selectedFeatures = map.GetSelection();
        //        var selectCount = linesLayer.SelectionCount;

        //        if (selectCount == 0)
        //        {
        //            MessageBox.Show("No Sewer Line was selected.\n\nTry selection again.");
        //        }
        //        else if (selectCount > 1)
        //        {
        //            MessageBox.Show("More than one Sewer Line was selected.\n" +
        //                "Try selecting Sewer Line again.\nZooming in may help.");
        //        }
        //        else
        //        {
        //            // get the first layer and its corresponding selected feature OIDs
        //            var firstSelectionSet = selectedFeatures.First();

        //            // create an instance of the inspector class
        //            var inspector = new Inspector();

        //            // load the selected features into the inspector using a list of object IDs
        //            inspector.Load(firstSelectionSet.Key, firstSelectionSet.Value);

        //            //get the value of
        //            return compkey = inspector["COMPKEY"].ToString();

        //        }

        //    }

        //    catch (Exception)
        //    {
        //        string caption = "SysModule.GetCompkey method failed!";
        //        string message = "Precess failed! \n\nSave and restart ArcGIS Pro and try process again.\n\n" +
        //            "If problem persist, contact your local GIS nerd.";

        //        //Using the ArcGIS Pro SDK MessageBox class
        //        MessageBox.Show(message, caption);

        //        return null;
        //    }

        //    return null;

        //}

        //public static string GetWHCompkey()
        //{
        //    string compkey;

        //    try
        //    {
        //        //ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New);

        //        var map = MapView.Active.Map;
        //        var linesLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((m => m.Name == "Sewer Lines"));
        //        var mhLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((m => m.Name == "Manholes"));

        //        // Get the currently selected features in the map
        //        var selectedFeatures = map.GetSelection();
        //        var linesSelCount = linesLayer.SelectionCount;
        //        var mhSelCount = mhLayer.SelectionCount;

        //        if (linesSelCount == 0 & mhSelCount == 0)
        //        {
        //            MessageBox.Show("No sewrer features were selected.\n\nTry selection again.");
        //        }
        //        else if (mhSelCount > 1 || linesSelCount > 1)
        //        {
        //            MessageBox.Show("More than one sewer feature was selected.\n" +
        //                "Try selecting feature again.\nZooming in may help.");
        //        }
        //        else
        //        {
        //            // get the first layer and its corresponding selected feature OIDs
        //            var firstSelectionSet = selectedFeatures.First();

        //            // create an instance of the inspector class
        //            var inspector = new Inspector();

        //            // load the selected features into the inspector using a list of object IDs
        //            inspector.Load(firstSelectionSet.Key, firstSelectionSet.Value);

        //            //get the value of
        //            return compkey = inspector["COMPKEY"].ToString();

        //        }

        //    }

        //    catch (Exception)
        //    {
        //        string caption = "SysModule.GetCompkey method failed!";
        //        string message = "Precess failed! \n\nSave and restart ArcGIS Pro and try process again.\n\n" +
        //            "If problem persist, contact your local GIS nerd.";

        //        //Using the ArcGIS Pro SDK MessageBox class
        //        MessageBox.Show(message, caption);

        //        return null;
        //    }

        //    return null;

        //}

        public static string GetManholeID()
        {
            try
            {
                string mhID;
                var map = MapView.Active.Map;
                var mhLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((m => m.Name == "Manholes"));

                // Get the currently selected features in the map
                var selectedFeatures = map.GetSelection();
                //var mhSelCount = mhLayer.SelectionCount;

                // get the first layer and its corresponding selected feature OIDs
                var firstSelectionSet = selectedFeatures.First();

                // create an instance of the inspector class
                var inspector = new Inspector();

                // load the selected features into the inspector using a list of object IDs
                inspector.Load(firstSelectionSet.Key, firstSelectionSet.Value);

                //get the value of
                return mhID = inspector["MH_NO"].ToString();
            }
            catch (Exception ex)
            {
                SysModule.LogError(ex.Message, ex.StackTrace);

                string caption = "Error Occured";
                string message = "Process failed.\nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
                return null;
            }
        }
        public static string GetPipeID()
        {
            try
            {
                string compkey;
                var map = MapView.Active.Map;
                var linesLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((m => m.Name == "Sewer Lines"));

                // Get the currently selected features in the map
                var selectedFeatures = map.GetSelection();
                var selectCount = linesLayer.SelectionCount;

                // get the first layer and its corresponding selected feature OIDs
                var firstSelectionSet = selectedFeatures.First();

                // create an instance of the inspector class
                var inspector = new Inspector();

                // load the selected features into the inspector using a list of object IDs
                inspector.Load(firstSelectionSet.Key, firstSelectionSet.Value);

                //get the value of
                return compkey = inspector["ARCID"].ToString();
            }
            catch (Exception ex)
            {

                SysModule.LogError(ex.Message, ex.StackTrace);

                string caption = "Error Occured";
                string message = "Process failed.\nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your GIS Admin.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
                return null;
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
