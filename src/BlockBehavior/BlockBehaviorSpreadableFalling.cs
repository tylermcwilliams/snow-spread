using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Vintagestory.GameContent
{
    public class BlockBehaviorSpreadableFalling : BlockBehavior
    {
        public BlockBehaviorSpreadableFalling(Block block) : base(block)
        {
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling)
        {
            handling = EnumHandling.NotHandled;

            if (blockSel != null && !world.BlockAccessor.GetBlock(blockSel.Position.DownCopy()).SideSolid[BlockFacing.UP.Index] && block.Attributes?["allowUnstablePlacement"].AsBool() != true)
            {
                handling = EnumHandling.PreventDefault;
                return false;
            }

            if (TryFalling(world, blockSel.Position))
            {
                handling = EnumHandling.PreventDefault;
            }

            return true;
        }

        public override void OnNeighourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos, ref EnumHandling handling)
        {
            base.OnNeighourBlockChange(world, pos, neibpos, ref handling);

            TryFalling(world, pos);
        }

        private bool TryFalling(IWorldAccessor world, BlockPos pos)
        {
            if (IsReplacableBeneath(world, pos))
            {
                // Prevents duplication
                Entity entity = world.GetNearestEntity(pos.ToVec3d().Add(0.5, 0.5, 0.5), 1, 3, (e) =>
                {
                    return e is EntityStackedBlockFalling && ((EntityStackedBlockFalling)e).initialPos.Equals(pos);
                }
                );

                if (entity == null)
                {
                    EntityStackedBlockFalling entityblock = new EntityStackedBlockFalling(block, world.BlockAccessor.GetBlockEntity(pos), pos);
                    world.SpawnEntity(entityblock);
                }

                return true;
            }

            return false;
        }

        private bool IsReplacableBeneath(IWorldAccessor world, BlockPos pos)
        {
            Block bottomBlock = world.BlockAccessor.GetBlock(pos.DownCopy());
            return (bottomBlock != null && bottomBlock.Replaceable > 6000);
        }
    }
}
