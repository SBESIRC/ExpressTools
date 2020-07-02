using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TopoNode
{
    public class ReferenceXclipInfo
    {
        public BlockReference curBlock; // 原始的blockReference
        //public BlockReference definedBlock; // 从块定义中读取的
        public List<XClipInfo> curXclips;

        public ReferenceXclipInfo(BlockReference memoryBlock, List<XClipInfo> clips = null)
        {
            curBlock = memoryBlock;

            if (clips == null)
                curXclips = new List<XClipInfo>();
            else
                curXclips = clips;
        }
    }

    public class XClipInfo
    {
        /// <summary>
        /// XClip边界点
        /// </summary>
        public Point2dCollection Pts { get; set; } = null;

        /// <summary>
        /// Xclip保留外部为true，保留内部为false
        /// </summary>
        public bool KeepExternal { get; set; } = true;

        // 当前xclip所在块中的矩阵变幻
        public Matrix3d curBlockTransform = Matrix3d.Identity;
    }

    public class Xclip
    {
        readonly static string filterDictName = "ACAD_FILTER";
        readonly static string spatialName = "SPATIAL";

        public class XmDwgFiler : DwgFiler
        {
            public FilerType m_FilerType;
            public ErrorStatus m_FilerStatus;

            public
#if AC2008
        int
#else
        long
#endif
        m_Position;

            //保存数据属性
            public List<IntPtr> AddressList { get; set; }
            public int AddressListPt = 0;
            public List<byte[]> BinaryChunkList { get; set; }
            public int BinaryChunkListPt = 0;
            public List<bool> BooleanList { get; set; }
            public int BooleanListPt = 0;
            public List<byte> ByteList { get; set; }
            public int ByteListPt = 0;
            public List<byte[]> BytesList { get; set; }
            public int BytesListPt = 0;
            public List<double> DoubleList { get; set; }
            public int DoubleListPt = 0;
            public List<Handle> HandleList { get; set; }
            public int HandleListPt = 0;
            public List<ObjectId> HardOwnershipIdList { get; set; }
            public int HardOwnershipIdListPt = 0;
            public List<ObjectId> HardPointerIdList { get; set; }
            public int HardPointerIdListPt = 0;
            public List<short> Int16List { get; set; }
            public int Int16ListPt = 0;
            public List<int> Int32List { get; set; }
            public int Int32ListPt = 0;
            public List<long> Int64List { get; set; }
            public int Int64ListPt = 0;
            public List<Point2d> Point2dList { get; set; }
            public int Point2dListPt = 0;
            public List<Point3d> Point3dList { get; set; }
            public int Point3dListPt = 0;
            public List<Scale3d> Scale3dList { get; set; }
            public int Scale3dListPt = 0;
            public List<ObjectId> SoftOwnershipIdList { get; set; }
            public int SoftOwnershipIdListPt = 0;
            public List<ObjectId> SoftPointerIdList { get; set; }
            public int SoftPointerIdListPt = 0;
            public List<string> StringList { get; set; }
            public int StringListPt = 0;
            public List<ushort> UInt16List { get; set; }
            public int UInt16ListPt = 0;
            public List<uint> UInt32List { get; set; }
            public int UInt32ListPt = 0;
            public List<ulong> UInt64List { get; set; }
            public int UInt64ListPt = 0;
            public List<Vector2d> Vector2dList { get; set; }
            public int Vector2dListPt = 0;
            public List<Vector3d> Vector3dList { get; set; }
            public int Vector3dListPt = 0;


            //构造函数
            public XmDwgFiler()
            {
                m_FilerType = FilerType.CopyFiler;
                m_FilerStatus = ErrorStatus.OK;
                m_Position = 0;
                AddressList = new List<IntPtr>();
                BinaryChunkList = new List<byte[]>();
                BooleanList = new List<bool>();
                ByteList = new List<byte>();
                BytesList = new List<byte[]>();
                DoubleList = new List<double>();
                HandleList = new List<Handle>();
                HardOwnershipIdList = new List<ObjectId>();
                HardPointerIdList = new List<ObjectId>();
                Int16List = new List<short>();
                Int32List = new List<int>();
                Int64List = new List<long>();
                Point2dList = new List<Point2d>();
                Point3dList = new List<Point3d>();
                Scale3dList = new List<Scale3d>();
                SoftOwnershipIdList = new List<ObjectId>();
                SoftPointerIdList = new List<ObjectId>();
                StringList = new List<string>();
                UInt16List = new List<ushort>();
                UInt32List = new List<uint>();
                UInt64List = new List<ulong>();
                Vector2dList = new List<Vector2d>();
                Vector3dList = new List<Vector3d>();
            }

            public override IntPtr ReadAddress()
            {
                if (AddressList.Count() == 0)
                {
                    return new IntPtr();
                }
                return AddressList[AddressListPt++];
            }

            public override byte[] ReadBinaryChunk()
            {
                if (BinaryChunkList.Count() == 0)
                {
                    return null;
                }
                return BinaryChunkList[BinaryChunkListPt++];
            }

            public override bool ReadBoolean()
            {
                if (BooleanList.Count() == 0)
                {
                    return false;
                }
                return BooleanList[BooleanListPt++];
            }

            public override byte ReadByte()
            {
                if (ByteList.Count() == 0)
                {
                    return 0;
                }
                return ByteList[ByteListPt++];
            }

            public override void ReadBytes(byte[] value)
            {
                if (ByteList.Count() == 0)
                {
                    return;
                }
                value = new byte[BytesList[BytesListPt].Length];
                BytesList[BytesListPt++].CopyTo(value, 0);
            }

            public override double ReadDouble()
            {
                if (DoubleList.Count() == 0)
                {
                    return 0;
                }
                return DoubleList[DoubleListPt++];
            }

            public override Handle ReadHandle()
            {
                if (HandleList.Count() == 0)
                {
                    return new Handle();
                }
                return HandleList[HandleListPt++];
            }

            public override ObjectId ReadHardOwnershipId()
            {
                if (HardOwnershipIdList.Count() == 0)
                {
                    return new ObjectId();
                }
                return HardOwnershipIdList[HardOwnershipIdListPt++];
            }

            public override ObjectId ReadHardPointerId()
            {
                if (HardPointerIdList.Count() == 0)
                {
                    return new ObjectId();
                }
                return HardPointerIdList[HardPointerIdListPt++];
            }

            public override short ReadInt16()
            {
                if (Int16List.Count() == 0)
                {
                    return 0;
                }
                return Int16List[Int16ListPt++];
            }

            public override int ReadInt32()
            {
                if (Int32List.Count() == 0)
                {
                    return 0;
                }
                return Int32List[Int32ListPt++];
            }

            public override Point2d ReadPoint2d()
            {
                if (Point2dList.Count() == 0)
                {
                    return new Point2d();
                }
                return Point2dList[Point2dListPt++];
            }

            public override Point3d ReadPoint3d()
            {
                if (Point3dList.Count() == 0)
                {
                    return new Point3d();
                }
                return Point3dList[Point3dListPt++];
            }

            public override Scale3d ReadScale3d()
            {
                if (Scale3dList.Count() == 0)
                {
                    return new Scale3d();
                }
                return Scale3dList[Scale3dListPt++];
            }

            public override ObjectId ReadSoftOwnershipId()
            {
                if (SoftOwnershipIdList.Count() == 0)
                {
                    return new ObjectId();
                }
                return SoftOwnershipIdList[SoftOwnershipIdListPt++];
            }

            public override ObjectId ReadSoftPointerId()
            {
                if (SoftPointerIdList.Count() == 0)
                {
                    return new ObjectId();
                }
                return SoftPointerIdList[SoftPointerIdListPt++];
            }

            public override string ReadString()
            {
                if (StringList.Count() == 0)
                {
                    return null;
                }
                return StringList[StringListPt++];
            }

            public override ushort ReadUInt16()
            {
                if (UInt16List.Count() == 0)
                {
                    return 0;
                }
                return UInt16List[UInt16ListPt++];
            }

            public override uint ReadUInt32()
            {
                if (UInt32List.Count() == 0)
                {
                    return 0;
                }
                return UInt32List[UInt32ListPt++];
            }

            public override Vector2d ReadVector2d()
            {
                if (Vector2dList.Count() == 0)
                {
                    return new Vector2d();
                }
                return Vector2dList[Vector2dListPt++];
            }

            public override Vector3d ReadVector3d()
            {
                if (Vector3dList.Count() == 0)
                {
                    return new Vector3d();
                }
                return Vector3dList[Vector3dListPt++];
            }

            public override void ResetFilerStatus()
            {
                AddressList.Clear();
                AddressListPt = 0;
                BinaryChunkList.Clear();
                BinaryChunkListPt = 0;
                BooleanList.Clear();
                BooleanListPt = 0;
                ByteList.Clear();
                ByteListPt = 0;
                BytesList.Clear();
                BytesListPt = 0;
                DoubleList.Clear();
                DoubleListPt = 0;
                HandleList.Clear();
                HandleListPt = 0;
                HardOwnershipIdList.Clear();
                HardOwnershipIdListPt = 0;
                HardPointerIdList.Clear();
                HardPointerIdListPt = 0;
                Int16List.Clear();
                Int16ListPt = 0;
                Int32List.Clear();
                Int32ListPt = 0;
                Int64List.Clear();
                Int64ListPt = 0;
                Point2dList.Clear();
                Point2dListPt = 0;
                Point3dList.Clear();
                Point3dListPt = 0;
                Scale3dList.Clear();
                Scale3dListPt = 0;
                SoftOwnershipIdList.Clear();
                SoftOwnershipIdListPt = 0;
                SoftPointerIdList.Clear();
                SoftPointerIdListPt = 0;
                StringList.Clear();
                StringListPt = 0;
                UInt16List.Clear();
                UInt16ListPt = 0;
                UInt32List.Clear();
                UInt32ListPt = 0;
                UInt64List.Clear();
                UInt64ListPt = 0;
                Vector2dList.Clear();
                Vector2dListPt = 0;
                Vector3dList.Clear();
                Vector3dListPt = 0;

                m_FilerType = FilerType.CopyFiler;
            }

            public override string ToString()
            {
                int ptCount =
                    AddressListPt +
                    BinaryChunkListPt +
                    BooleanListPt +
                    ByteListPt +
                    BytesListPt +
                    DoubleListPt +
                    HandleListPt +
                    HardOwnershipIdListPt +
                    HardPointerIdListPt +
                    Int16ListPt +
                    Int32ListPt +
                    Int64ListPt +
                    Point2dListPt +
                    Point3dListPt +
                    Scale3dListPt +
                    SoftOwnershipIdListPt +
                    SoftPointerIdListPt +
                    StringListPt +
                    UInt16ListPt +
                    UInt32ListPt +
                    UInt64ListPt +
                    Vector2dListPt +
                    Vector3dListPt;
                int ltCount =
                    AddressList.Count() +
                    BinaryChunkList.Count() +
                    BooleanList.Count() +
                    ByteList.Count() +
                    BytesList.Count() +
                    DoubleList.Count() +
                    HandleList.Count() +
                    HardOwnershipIdList.Count() +
                    HardPointerIdList.Count() +
                    Int16List.Count() +
                    Int32List.Count() +
                    Int64List.Count() +
                    Point2dList.Count() +
                    Point3dList.Count() +
                    Scale3dList.Count() +
                    SoftOwnershipIdList.Count() +
                    SoftPointerIdList.Count() +
                    StringList.Count() +
                    UInt16List.Count() +
                    UInt32List.Count() +
                    UInt64List.Count() +
                    Vector2dList.Count() +
                    Vector3dList.Count();

                return
                    "\n" + ptCount.ToString() + " DataIn" +
                    "\n" + ltCount.ToString() + " DataOut";
            }

            public override void WriteAddress(IntPtr value)
            {
                AddressList.Add(value);
            }

            public override void WriteBinaryChunk(byte[] chunk)
            {
                BinaryChunkList.Add(chunk);
            }

            public override void WriteBoolean(bool value)
            {
                BooleanList.Add(value);
            }

            public override void WriteByte(byte value)
            {
                ByteList.Add(value);
            }

            public override void WriteBytes(byte[] value)
            {
                BytesList.Add(value);
            }

            public override void WriteDouble(double value)
            {
                DoubleList.Add(value);
            }

            public override void WriteHandle(Handle handle)
            {
                HandleList.Add(handle);
            }

            public override void WriteHardOwnershipId(ObjectId value)
            {
                HardOwnershipIdList.Add(value);
            }

            public override void WriteHardPointerId(ObjectId value)
            {
                HardPointerIdList.Add(value);
            }

            public override void WriteInt16(short value)
            {
                Int16List.Add(value);
            }

            public override void WriteInt32(int value)
            {
                Int32List.Add(value);
            }

            public override void WritePoint2d(Point2d value)
            {
                Point2dList.Add(value);
            }

            public override void WritePoint3d(Point3d value)
            {
                Point3dList.Add(value);
            }

            public override void WriteScale3d(Scale3d value)
            {
                Scale3dList.Add(value);
            }

            public override void WriteSoftOwnershipId(ObjectId value)
            {
                SoftOwnershipIdList.Add(value);
            }

            public override void WriteSoftPointerId(ObjectId value)
            {
                SoftPointerIdList.Add(value);
            }

            public override void WriteString(string value)
            {
                StringList.Add(value);
            }

            public override void WriteUInt16(ushort value)
            {
                UInt16List.Add(value);
            }

            public override void WriteUInt32(uint value)
            {
                UInt32List.Add(value);
            }

            public override void WriteVector2d(Vector2d value)
            {
                Vector2dList.Add(value);
            }

            public override void WriteVector3d(Vector3d value)
            {
                Vector3dList.Add(value);
            }

            public override ErrorStatus FilerStatus
            {
                get
                {
                    return m_FilerStatus;
                }
                set
                {
                    m_FilerStatus = value;
                }
            }

            public override FilerType FilerType
            {
                get
                {
                    return this.m_FilerType;
                }
            }

#if !AC2008
            public override long ReadInt64()
            {
                if (Int64List.Count() == 0)
                {
                    return 0;
                }
                return Int64List[Int64ListPt++];
            }

            public override ulong ReadUInt64()
            {
                if (UInt64List.Count() == 0)
                {
                    return 0;
                }
                return UInt64List[UInt64ListPt++];
            }

            public override void WriteInt64(long value)
            {
                Int64List.Add(value);
            }

            public override void WriteUInt64(ulong value)
            {
                UInt64List.Add(value);
            }
#endif

            //https://www.eabim.net//forum.php/?mod=viewthread&tid=169503&extra=page%3D1&page=1&
            public override void Seek(
#if AC2008
       int
#else
        long
#endif
         offset, int method)
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            }

            public override
#if AC2008
        int
#else
         long
#endif
        Position
            {
                get
                {
                    return m_Position;
                }
            }
        }

        /// <summary>
        /// 检测块中是否有XClip
        /// </summary>
        /// <param name="blockRef"></param>
        /// <returns></returns>
        public static XClipInfo RetrieveXClipBoundary(BlockReference blockRef)
        {
            var _document = Application.DocumentManager.MdiActiveDocument;
            Transaction _trans = _document.Database.TransactionManager.TopTransaction;
            XClipInfo xClipInfo = new XClipInfo();
            if (blockRef == null || blockRef.ExtensionDictionary == ObjectId.Null)
            {
                return null;
            }
            try
            {
                // The extension dictionary needs to contain a nested
                // dictionary called ACAD_FILTER
                var extdict = _trans.GetObject(blockRef.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                if (extdict != null && extdict.Contains(filterDictName))
                {
                    var fildict = _trans.GetObject(extdict.GetAt(filterDictName), OpenMode.ForRead) as DBDictionary;
                    if (fildict != null)
                    {
                        // The nested dictionary should contain a
                        // SpatialFilter object called SPATIAL
                        if (fildict.Contains(spatialName))
                        {
                            SpatialFilter fil = _trans.GetObject(fildict.GetAt(spatialName), OpenMode.ForRead) as SpatialFilter;
                            if (fil != null && fil.Definition.Enabled)
                            {

                                bool isInverted = true; // 保留内部为false, 保留外部为true
#if ACAD2012 || ACAD2014
                                XmDwgFiler xmDwgFiler;
                                isInverted = Inverted(fil, out xmDwgFiler);
#else
                                isInverted = fil.Inverted;
#endif
                                xClipInfo.Pts = fil.Definition.GetPoints();
                                Point2dCollection pts = new Point2dCollection();
                                foreach (Point2d pt in fil.Definition.GetPoints())
                                {
                                    Point3d tempPt = new Point3d(pt.X, pt.Y, 0.0);
                                    tempPt = tempPt.TransformBy(fil.ClipSpaceToWorldCoordinateSystemTransform); //从Ucs到Wcs
                                    tempPt = tempPt.TransformBy(fil.OriginalInverseBlockTransform); //转到当前块定义中的坐标点
                                    pts.Add(new Point2d(tempPt.X, tempPt.Y));
                                }

                                xClipInfo.Pts = pts;
                                xClipInfo.KeepExternal = isInverted;
                                xClipInfo.curBlockTransform = blockRef.BlockTransform;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
            }

            if (xClipInfo.Pts == null)
                return null;

            return xClipInfo;
        }

        public static bool Inverted(SpatialFilter spatialFilter, out XmDwgFiler xmDwgFiler)
        {
            xmDwgFiler = new XmDwgFiler();
            spatialFilter.DwgOut(xmDwgFiler);
            var f = xmDwgFiler.UInt16List[1];
            if (f != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
