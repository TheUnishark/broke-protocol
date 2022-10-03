﻿using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.GameSource.Types;
using BrokeProtocol.Utility;
using System.Text;
using UnityEngine;

namespace BrokeProtocol.GameSource
{
    public class Army : LoopJob
    {
        public override void SetJob()
        {
            base.SetJob();
            if (player.isHuman)
            {
                foreach (var territory in Manager.territories)
                {
                    territory.svEntity.AddSubscribedPlayer(player);
                }
            }
        }

        public override void RemoveJob()
        {
            if (player.isHuman)
            {
                foreach (var territory in Manager.territories)
                {
                    territory.svEntity.RemoveSubscribedPlayer(player, true);
                }
            }
            base.RemoveJob();
        }

        public override void Loop()
        {
            if (!player.isHuman && player.IsMobile)
            {
                TryFindEnemy();
            }
        }

        protected bool IsEnemy(ShPlayer target) => this != target.svPlayer.job;

        protected bool TryFindMount()
        {
            return player.svPlayer.LocalEntitiesOne(
                (e) => e is ShMountable p && p.IsAccessible(player, true) && !p.occupants[0],
                (e) =>
                {
                    player.svPlayer.targetEntity = e;
                    player.svPlayer.SetState(WarCore.Mount.index);
                });
        }

        protected bool TryFindLeader()
        {
            return player.svPlayer.LocalEntitiesOne(
                (e) => e is ShPlayer p && !p.curMount && !p.svPlayer.follower && p.IsMobile,
                (e) =>
                {
                    player.svPlayer.targetEntity = e;
                    player.svPlayer.SetState(Core.Follow.index);
                });
        }

        public override void ResetJobAI()
        {
            player.svPlayer.SetBestWeapons();

            // TODO: Smarter goal selection

            if (player.IsFlying || player.IsBoating) // WaypointState if in a boat or aircraft
            {
                if(player.svPlayer.SetState(Core.Waypoint.index))
                    return;
            }

            if(!player.curMount && Random.value < 0.2f && TryFindMount())
            {
                return;
            }

            if (!player.svPlayer.leader && Random.value < 0.2f && TryFindLeader()) // Follow a teammate
            {
                return;
            }

            var territoryIndex = Random.Range(0, Manager.territories.Count);

            if (WarUtility.GetValidTerritoryPosition(territoryIndex, out var pos, out var rot, out var place))
            {
                // Overwatch a territory
                if (Random.value < 0.5f && player.svPlayer.GetOverwatchBest(pos, out var goal) &&
                    player.GamePlayer().SetGoToState(goal, rot, place.mTransform))
                {
                    return;
                }
                else if (player.svPlayer.GetOverwatchSafe(pos, Manager.territories[territoryIndex].mainT.GetWorldBounds(), out var goal2) &&
                    player.GamePlayer().SetGoToState(goal2, rot, place.mTransform))
                {
                    return;
                }
            }

            // Nothing else to really do, maybe a timed WanderState?
            player.svPlayer.DestroySelf();
        }

        public void TryFindEnemy()
        {
            player.svPlayer.LocalEntitiesOne(
                (e) =>
                {
                    var p = e.Player;
                    if (p && p.IsCapable && IsEnemy(p) && player.CanSeeEntity(e, true))
                    {
                        if (!player.svPlayer.targetEntity)
                            return true;

                        return player.DistanceSqr(e) <
                        0.5f * player.DistanceSqr(player.svPlayer.targetEntity);
                    }
                    return false;
                },
                (e) =>
                {
                    player.GamePlayer().SetAttackState(e);
                });
        }

        public override void OnDestroyEntity(ShEntity destroyed)
        {
            base.OnDestroyEntity(destroyed);
            var victim = destroyed.Player;
            if (victim)
            {
                if (IsEnemy(victim))
                {
                    // TODO: Reward players for stuff other than killing
                    player.svPlayer.Reward(1, 0);
                    // Ticket burn
                    var victimIndex = victim.svPlayer.job.info.shared.jobIndex;

                    WarManager.tickets[victimIndex] -= 1f;

                    if (victim.isHuman && player.isHuman)
                    {
                        InterfaceHandler.SendGameMessageToAll(KillString(player, victim, " killed "));
                    }
                }
                else if (player.isHuman)
                {
                    InterfaceHandler.SendGameMessageToAll(KillString(player, victim, " &4team-killed "));
                }
            }
        }

        private string KillString(ShPlayer attacker, ShPlayer victim, string s)
        {
            var sb = new StringBuilder();
            sb.AppendColorText(attacker.username, attacker.svPlayer.job.info.shared.GetColor());
            sb.Append(s);
            sb.AppendColorText(victim.username, victim.svPlayer.job.info.shared.GetColor());

            return sb.ToString();
        }
    }
}
