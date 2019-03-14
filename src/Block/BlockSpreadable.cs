using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent
{
    public class BlockSpreadable : Block
    {
        public Block GetNextLayer(IWorldAccessor world)
        {
            int layer = 0;
            int.TryParse(Code.Path.Split('-')[1], out layer);

            string basecode = CodeWithoutParts(1);

            if (layer < 7) return world.BlockAccessor.GetBlock(CodeWithPath(basecode + "-" + (layer + 1)));
            return world.BlockAccessor.GetBlock(CodeWithPath(basecode.Replace("layer", "block")));
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel)
        {
            if (!world.TryAccessBlock(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
            {
                byPlayer.InventoryManager.ActiveHotbarSlot.MarkDirty();
                return false;
            }

            Block block = world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(blockSel.Face.GetOpposite()));

            if (block is BlockSpreadable)
            {
                Block nextBlock = ((BlockSpreadable)block).GetNextLayer(world);
                world.BlockAccessor.SetBlock(nextBlock.BlockId, blockSel.Position.AddCopy(blockSel.Face.GetOpposite()));

                return true;
            }

            block = world.BlockAccessor.GetBlock(blockSel.Position);

            if (!CanLayerStay(world, blockSel.Position))
            {
                return false;
            }

            base.TryPlaceBlock(world, byPlayer, itemstack, blockSel);
            return true;
        }

        #region
        // Falling stack
        public Block[] GenResultingLayers(IWorldAccessor world, int size)
        {
            Block[] blockStack = new Block[2];
            int layer = 0;
            int.TryParse(Code.Path.Split('-')[1], out layer);

            string basecode = CodeWithoutParts(1);
            
            if(layer + size < 8)
            {
                blockStack[0] = world.BlockAccessor.GetBlock(CodeWithPath(basecode + "-" + (layer + size)));
                blockStack[1] = null;
                return blockStack;
            }
            else
            {
                blockStack[0] = world.BlockAccessor.GetBlock(CodeWithPath(basecode.Replace("layer", "block")));
                size = ((layer + size)-8);
                if(size > 0)
                {
                    blockStack[1] = world.BlockAccessor.GetBlock(CodeWithPath(basecode + "-" + (size)));
                } else
                {
                    blockStack[1] = null;
                }
                return blockStack;
            }
        }

        public void FallBlock(IWorldAccessor world, BlockPos pos, int size)
        {
            Block block = world.BlockAccessor.GetBlock(pos.AddCopy(0));

            Block[] stackedBlocks = ((BlockSpreadable)block).GenResultingLayers(world, size);

            world.BlockAccessor.SetBlock(stackedBlocks[0].BlockId, pos.AddCopy(0));

            if(stackedBlocks[1] != null)
            {
                world.BlockAccessor.SetBlock(stackedBlocks[1].BlockId, pos.AddCopy(0, 1, 0));
            }
        }

        #endregion

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            Block block = world.BlockAccessor.GetBlock(CodeWithParts("1"));
            return new ItemStack(block);
        }


        public override void OnNeighourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if (GetBehavior<BlockBehaviorSpreadableFalling>() != null)
            {
                base.OnNeighourBlockChange(world, pos, neibpos);
                return;
            }

            if (!CanLayerStay(world, pos))
            {
                world.BlockAccessor.BreakBlock(pos, null);
            }
        }

        bool CanLayerStay(IWorldAccessor world, BlockPos pos)
        {
            BlockPos belowPos = pos.DownCopy();
            Block block = world.BlockAccessor.GetBlock(world.BlockAccessor.GetBlockId(belowPos));

            return block.CanAttachBlockAt(world.BlockAccessor, this, belowPos, BlockFacing.UP);
        }

        public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace)
        {
            return false;
        }
    }
}