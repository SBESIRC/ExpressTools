﻿using AcHelper;
using DotNetARX;
using Autodesk.AutoCAD.Runtime;

namespace ThMirror
{
    public class ThMirrorApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        // 天华镜像
        //  基于下面一个事实：
        //  CAD自动的"MIRROR"命令，在MIRRTEXT=0情况下，对于文字实体，镜像后的结果正是我们需要的。
        //  实现原理：
        //  通过各种技术手段（Events，Overrule），“监听”镜像命令整个过程。
        //  对于其中包含有文字的块引用，通过将块引用“炸成”基本图元，并对这个基本图元完成相同的镜像操作。
        //  在镜像命令结束后，对这些基本图元再根据原先的块结构还原成新的块，并用新创建的块作为镜像后的结果。
        //  复杂情况：
        //      1. 多层嵌套块
        //      2. 动态块
        //      3. 外部参照(Xref）
        [CommandMethod("TIANHUACAD", "THMIR", CommandFlags.Transparent)]
        public void ThMirror()
        {
            // 注册命令事件
            ThMirrorDocumentReactor.Instance.SubscribeToDoc(Active.Document);

            // 异步运行“镜像”命令
            Active.Editor.PostCommand("_.MIRROR ");
        }
    }
}