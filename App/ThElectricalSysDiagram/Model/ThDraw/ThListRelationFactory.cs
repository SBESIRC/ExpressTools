using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThElectricalSysDiagram
{
    public class ThListRelationFactory
    {
        public static List<ThRelationInfo> CreateListThRelations(string ruleName, ThSysDiagramViewModel viewModel)
        {
            switch (ruleName)
            {
                case "按图层转换":
                    return viewModel.RelationFanInfos.Cast<ThRelationInfo>().ToList();
                case "按图块转换":
                    return viewModel.RelationBlockInfos.Cast<ThRelationInfo>().ToList();
                default:
                    return null;
            }
        }
    }
}
