using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace LRWRA
{
    internal class SelectSewersTool : MapTool
    {
        public SelectSewersTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Lasso;
            SketchOutputMode = SketchOutputMode.Screen;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return QueuedTask.Run(() =>
            {
                SysModule.SewersStatus();

            });
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return QueuedTask.Run(() =>
            {
                try
                {
                    ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.Add);
                }

                catch (Exception ex)
                {
                    SysModule.LogError(ex.Message, ex.StackTrace);

                    string caption = "Failed to Select Features";
                    string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                        "If problem persist, contact your GIS Admin.";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }
                return true;
            });
        }
    }
}
