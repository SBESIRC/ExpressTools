using System;
using TopoNode;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThWSS.Engine
{
    public class ThRoomLayerManager
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThRoomLayerManager instance = new ThRoomLayerManager();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThRoomLayerManager() { }
        internal ThRoomLayerManager() { }
        public static ThRoomLayerManager Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        private Dictionary<string, List<string>> layers;
        public void Initialize()
        {
            var allCurveLayers = Utils.ShowThLayers(
                out List<string> wallLayers,
                out List<string> arcDoorLayers,
                out List<string> windLayers,
                out List<string> validLayers,
                out List<string> beamLayers,
                out List<string> columnLayers);
            layers = new Dictionary<string, List<string>>()
            {
                { "All", allCurveLayers},
                { "Valid", validLayers},
                { "Wall", wallLayers},
                { "Door", arcDoorLayers},
                { "Window", windLayers}
            };
        }

        public List<string> ValidLayers()
        {
            return layers["Valid"];
        }

        public List<string> AllLayers()
        {
            return layers["All"];
        }

        public List<string> WallLayers()
        {
            return layers["Wall"];
        }

        public List<string> WindowLayers()
        {
            return layers["Window"];
        }

        public List<string> DoorLayers()
        {
            return layers["Door"];
        }
    }
}
