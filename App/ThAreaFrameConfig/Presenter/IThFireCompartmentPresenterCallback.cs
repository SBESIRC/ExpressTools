using System;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThFireCompartmentPresenterCallback
    {
        // 选取面积框线
        bool OnPickAreaFrames(ThFireCompartment compartment, string layer, string islandLayer);

        // 修改防火分区
        bool OnModifyFireCompartment(ThFireCompartment compartment);
        bool OnModifyFireCompartments(List<ThFireCompartment> compartments);

        // 合并防火分区
        bool OnMergeFireCompartments(List<ThFireCompartment> compartments);

        // 删除防火分区
        bool OnDeleteFireCompartments(List<ThFireCompartment> compartments);

        // 创建防火分区疏散宽度表
        void OnCreateFCCommerceTable(ThFCCommerceSettings settings);
        void OnCreateFCUndergroundParkingTable(ThFCUnderGroundParkingSettings settings);

        // 创建防火分区填充
        void OnCreateFireCompartmentFills(List<ThFireCompartment> compartments);

        // 拾取防火分区外轮廓线图层
        bool OnSetFireCompartmentLayer(ThFireCompartmentSettings settings, string key);

        // 拾取防火分区并合并
        bool OnMergePickedFireCompartments(ThFireCompartmentSettings settings);
    }
}
