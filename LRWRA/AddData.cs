using System;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;



namespace LRWRA
{
    internal class AddData : Button
    {   
        protected async override void OnClick()
        {
            try
            { 
                OpenItemDialog addToMap = new OpenItemDialog
                {
                    Title = "Add Data",
                    InitialLocation = @"O:\SHARE\405 - INFORMATION SERVICES\GIS_Layers\Production",
                    MultiSelect = true
                };


                bool? ok = addToMap.ShowDialog();

                if (ok == true)
                {
                    IEnumerable<Item> selectedItems = addToMap.Items;
                    foreach (Item selectedItem in selectedItems)
                    {
                        await QueuedTask.Run(() => 
                            LayerFactory.Instance.CreateLayer(selectedItem, MapView.Active.Map, LayerPosition.AutoArrange));
                    }
                }
            }

            catch (Exception ex)
            {
                SysModule.LogError(ex.Message, ex.StackTrace);

                string caption = "Failed to add data";
                string message = "Process failed.\n\nSave and restart ArcGIS Pro and try process again.\n\n" +
                    "If problem persist, contact your local GIS nerd.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }
        }
    }
}
