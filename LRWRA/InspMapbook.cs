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
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Layouts;

namespace LRWRA_AI
{
    internal class InspMapbook: Button
    {
        protected override void OnClick()
        {
            LayoutProjectItem layout = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("Layout-Inspections"));

            if (layout != null)
            {
                var toolPath = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\Python-Tools\Acoustic-Inspections\AI-Mapbooks.tbx\InspectionsExport";

                string[] args = new string[0];
                Geoprocessing.OpenToolDialog(toolPath, args);
            }
            else
            {
                MessageBox.Show("'Layout-Inspections' not in currect ArcGIS Pro project. This layout and associated map must be in this project for the 'Inspections Mapbook' tool to function.", "Layout Missing!");
            }

        }
    }
}
