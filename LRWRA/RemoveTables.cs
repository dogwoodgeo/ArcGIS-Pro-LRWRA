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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRWRA
{
    internal class RemoveTables : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                try
                {
                    var map = MapView.Active.Map;//assumes non-null

                    //get the tables from the map container
                    var saTables = map.GetStandaloneTablesAsFlattenedList();
                    //delete the first...
                    if (saTables.Count() == 0)
                    {
                        MessageBox.Show("Map does not have any standalone tables.", "Warning");

                    }

                    else
                    {
                        map.RemoveStandaloneTables(saTables);
                    }
                }

                catch (Exception ex)
                {
                    SysModule.LogError(ex.Message, ex.StackTrace);

                    string caption = "Failed to Load Tables";
                    string message = "Process failed. \nSave and restart ArcGIS Pro and try process again.\n" +
                        "If problem persist, contact your GIS Admin.";

                    //Using the ArcGIS Pro SDK MessageBox class
                    MessageBox.Show(message, caption);
                }

            });


        }
    }
}
