using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ThResourceLibrary
{
    public class CustomResources
    {
        public static ComponentResourceKey SadTileBrush
        {
            get
            {
                return new ComponentResourceKey(
                    typeof(CustomResources), "SadTileBrush");
            }
        }

        public static ComponentResourceKey ThConfirmButtonStyle
        {
            get
            {
                return new ComponentResourceKey(
                    typeof(CustomResources), "ThConfirmButtonStyle");
            }
        }

        public static ComponentResourceKey ThGroupStyle
        {
            get
            {
                return new ComponentResourceKey(
                    typeof(CustomResources), "ThGroupStyle");
            }
        }

        public static ComponentResourceKey ThBlockThubnailTemplate
        {
            get
            {
                return new ComponentResourceKey(
                    typeof(CustomResources), "ThBlockThubnailTemplate");
            }
        }


        public static ComponentResourceKey ThDataGridStyle
        {
            get
            {
                return new ComponentResourceKey(
                    typeof(CustomResources), "ThDataGridStyle");
            }
        }

        public static ComponentResourceKey ThSelectButtonTemplate
        {
            get
            {
                return new ComponentResourceKey(
                    typeof(CustomResources), "ThSelectButtonTemplate");
            }
        }
    }
}
