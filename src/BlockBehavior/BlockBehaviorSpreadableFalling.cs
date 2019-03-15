using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
            } else
            {
                TrySpreading(world, blockSel.Position);
            }

            return true;
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handled)
        {
            if (TryFalling(world, blockPos))
            {
                handled = EnumHandling.PreventDefault;
            }
            else
            {
                TrySpreading(world, blockPos);
            }
            base.OnBlockPlaced(world, blockPos, ref handled);
        }

        public override void OnNeighourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos, ref EnumHandling handling)
        {
            base.OnNeighourBlockChange(world, pos, neibpos, ref handling);

            if (!TryFalling(world, pos)) {
                TrySpreading(world, pos);
            };

        }

        // Spread method
        public bool TrySpreading(IWorldAccessor world, BlockPos pos)
        {
            // Stack value

            int layers = 0;
            int.TryParse(block.LastCodePart(0), out layers);

            List<KeyValuePair<BlockPos, int>> neighbors = new List<KeyValuePair<BlockPos, int>>();
            List<KeyValuePair<BlockPos, int>> spreadNeighbors = new List<KeyValuePair<BlockPos, int>>();
    

            neighbors.Add(CanSpreadTo(world, pos.SouthCopy()));
            neighbors.Add(CanSpreadTo(world, pos.WestCopy()));
            neighbors.Add(CanSpreadTo(world, pos.NorthCopy()));
            neighbors.Add(CanSpreadTo(world, pos.EastCopy()));

            neighbors.Sort((pair1, pair2) =>
            {
                return pair1.Value.CompareTo(pair2.Value);
            });

            int smallest = neighbors[0].Value;
            while(layers > smallest + 1)
            {
                neighbors.Sort((pair1, pair2) =>
                {
                    return pair1.Value.CompareTo(pair2.Value);
                });

                smallest = neighbors[0].Value;

                KeyValuePair<BlockPos, int> item = neighbors.Find((entry) =>
                {
                    return entry.Value == smallest;
                });
                
                    if (item.Value != smallest)
                    {
                        break;
                    }

                    int index = neighbors.FindIndex((entry) =>
                    {
                        return entry.Key == item.Key;
                    }); 
                    
                    neighbors[index] = new KeyValuePair<BlockPos, int>(item.Key, item.Value +1);
                    layers--;
            }
            

            // Block type
            string basecode = block.CodeWithoutParts(1);

            neighbors.ForEach((entry) =>
            {
                if (entry.Value <= 0 || entry.Value >= 8)
                {
                    return;
                };
                Block newBlock = world.BlockAccessor.GetBlock(block.CodeWithPath(basecode + "-" + (entry.Value)));
                world.BlockAccessor.SetBlock(newBlock.BlockId, entry.Key);
            });

            Block originBlock = world.BlockAccessor.GetBlock(block.CodeWithPath(basecode + "-" + layers));
            world.BlockAccessor.SetBlock(originBlock.BlockId, pos);

            return false;
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

        private KeyValuePair<BlockPos, int> CanSpreadTo(IWorldAccessor world, BlockPos pos)
        {
            Block targetBlock = world.BlockAccessor.GetBlock(pos);
            if(targetBlock != null && targetBlock.FirstCodePart(2) == block.FirstCodePart(2))
            {
                int layer = 0;
                int.TryParse(targetBlock.LastCodePart(), out layer);
                return new KeyValuePair<BlockPos, int>(pos, layer);
            } else if(targetBlock.Replaceable > 6000 && targetBlock.BlockMaterial != EnumBlockMaterial.Liquid)
            {
                return new KeyValuePair<BlockPos, int>(pos, 0);
            } else
            {

            return new KeyValuePair<BlockPos, int>(pos, 8); ;
            }
        }
  
    }
}
