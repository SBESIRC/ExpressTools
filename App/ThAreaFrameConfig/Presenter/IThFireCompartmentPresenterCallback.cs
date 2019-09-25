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

        // 合并防火分区
        bool OnMergeFireCompartments(List<ThFireCompartment> compartments);

        // 删除防火分区
        bool OnDeleteFireCompartments(List<ThFireCompartment> compartments);

        // 创建商业防火分区疏散宽度表
        void OnCreateFCCommerceTable(ThFCCommerceSettings settings);
    }
}
