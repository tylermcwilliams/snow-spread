using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace snowspread.src
{
    public class Core : ModSystem
    {
        
        public static ICoreServerAPI sapi;

        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockSpreadable", typeof(BlockSpreadable));
            api.RegisterBlockBehaviorClass("StackedUnstableFalling", typeof(BlockBehaviorSpreadableFalling));
        }

    }
}