﻿using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.Required;
using BrokeProtocol.Utility.Networking;
using UnityEngine;

namespace BrokeProtocol.GameSource.Types
{
    public class Movable : Destroyable
    {
        [Target(GameSourceEvent.MovableUpdate)]
        protected void OnUpdate(ShMovable movable)
        {
            base.OnUpdate(movable);
        }

        [Target(GameSourceEvent.MovableFixedUpdate)]
        protected void OnFixedUpdate(ShMovable movable)
        {
            base.OnFixedUpdate(movable);
        }

        [Target(GameSourceEvent.MovableDamage)]
        protected void OnDamage(ShMovable movable, DamageIndex damageIndex, float amount, ShPlayer attacker, Collider collider)
        {
            base.OnDamage(movable, damageIndex, amount, attacker, collider);

            movable.svMovable.Send(SvSendType.Local,
                Channel.Reliable,
                ClPacket.UpdateHealth,
                movable.ID,
                movable.health);
        }

        [Target(GameSourceEvent.MovableDeath)]
        protected void OnDeath(ShMovable movable)
        {
            if (movable.svMovable.respawnable)
            {
                movable.StartCoroutine(movable.svMovable.RespawnDelay());
            }
            else
            {
                movable.StartCoroutine(movable.DestroyDelay(movable.svMovable.RespawnTime));
            }
        }

        [Target(GameSourceEvent.MovableRespawn)]
        protected void OnRespawn(ShMovable movable)
        {
            if (movable.svMovable.randomSpawn)
            {
                movable.svMovable.Despawn();
            }
            else if (movable.IsDead)
            {
                movable.svMovable.Send(SvSendType.Local, Channel.Reliable, ClPacket.Spawn,
                    movable.ID,
                    movable.originalPosition,
                    movable.originalRotation,
                    movable.svMovable.GetOriginalParentIndex);
                movable.Spawn(movable.originalPosition, movable.originalRotation, movable.originalParent);
            }
            else
            {
                movable.svMovable.ResetOriginal();
            }
        }
    }
}