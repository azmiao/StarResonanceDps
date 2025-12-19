using BlueProto;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using StarResonanceDpsAnalysis.Core.Analyze.Models;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Extends.System;

namespace StarResonanceDpsAnalysis.Core.Analyze.V2.Processors;

/// <summary>
/// Processes the SyncNearEntities message to update player and enemy attributes.
/// </summary>
internal sealed class SyncNearEntitiesProcessor : IMessageProcessor
{
    private readonly IDataStorage _storage;
    private readonly ILogger? _logger;
    private readonly List<Action<long, RepeatedField<Attr>>?> _entitySyncHandlers;

    public SyncNearEntitiesProcessor(IDataStorage storage, ILogger? logger)
    {
        _storage = storage;
        _logger = logger;
        _entitySyncHandlers =
        [
            null,
            ProcessEnemyAttrs, // EEntityType.EntMonster(1)
            null, null, null, null, null, null, null, null,
            ProcessPlayerAttrs // EEntityType.EntChar(10)
        ];
    }

    public void Process(byte[] payload)
    {
        _logger?.LogDebug("Sync Near entities");
        var syncNearEntities = SyncNearEntities.Parser.ParseFrom(payload);
        if (syncNearEntities.Appear == null || syncNearEntities.Appear.Count == 0) return;

        foreach (var entity in syncNearEntities.Appear)
        {
            if ((int)entity.EntType < 0 || (int)entity.EntType >= _entitySyncHandlers.Count) continue;

            var handler = _entitySyncHandlers[(int)entity.EntType];
            if (handler == null) continue;

            var attrCollection = entity.Attrs;
            if (attrCollection?.Attrs == null || attrCollection.Attrs.Count == 0) continue;

            var entityUid = entity.Uuid.ShiftRight16();
            if (entityUid == 0) continue;

            handler(entityUid, attrCollection.Attrs);
        }
    }

    private void ProcessPlayerAttrs(long playerUid, RepeatedField<Attr> attrs)
    {
        _storage.EnsurePlayer(playerUid);

        foreach (var attr in attrs)
        {
            if (attr.Id == 0 || attr.RawData == null || attr.RawData.Length == 0) continue;
            var reader = new CodedInputStream(attr.RawData.ToByteArray());

            var attrType = (AttrType)attr.Id;
            if (!Enum.IsDefined(attrType))
            {
#if DEBUG
                _logger?.LogWarning("Unknown attribute type: {AttrType}", attrType);
#else
                _logger?.LogTrace("Unknown attribute type: {AttrType}", attrType);
#endif
                continue;
            }
            switch (attrType)
            {
                case AttrType.AttrName:
                    var playerName = reader.ReadString();
                    _storage.SetPlayerName(playerUid, playerName);
                    _logger?.LogWarning("Set PlayerName: {playerUid}@{playerName}", playerUid, playerName);
                    break;
                case AttrType.AttrProfessionId:
                    _storage.SetPlayerProfessionID(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrFightPoint:
                    _storage.SetPlayerCombatPower(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrLevel:
                    _storage.SetPlayerLevel(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrRankLevel:
                    _storage.SetPlayerRankLevel(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrCri:
                    _storage.SetPlayerCritical(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrLucky:
                    _storage.SetPlayerLucky(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrHp:
                    _storage.SetPlayerHP(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrMaxHp:
                    _storage.SetPlayerMaxHP(playerUid, reader.ReadInt32());
                    break;

                case AttrType.SeasonStrength:
                    _storage.SetPlayerSeasonStrength(playerUid, reader.ReadInt32());
                    break;
                case AttrType.SeasonLevel:
                    _storage.SetPlayerSeasonLevel(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrElementFlag:
                    _storage.SetPlayerElementFlag(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrReductionLevel:
                    _storage.SetPlayerReductionLevel(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrEnergyFlag:
                    _storage.SetPlayerEnergyFlag(playerUid, reader.ReadInt32());
                    break;
                case AttrType.AttrId:
                case AttrType.AttrReduntionId:
                    break;
                    // default:
                    //     throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void ProcessEnemyAttrs(long enemyUid, RepeatedField<Attr> attrs)
    {
        if (attrs.Count == 0) return;

        _storage.EnsurePlayer(enemyUid);

        foreach (var attr in attrs)
        {
            if (attr.Id == 0 || attr.RawData == null || attr.RawData.Length == 0) continue;

            var rawBytes = attr.RawData.ToByteArray();
            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                var rawBase64 = Convert.ToBase64String(rawBytes);
                _logger.LogDebug("Found attrId {AttrId} for enemy {EnemyUid} {AttrRaw}", attr.Id, enemyUid, rawBase64);
            }

            var reader = new CodedInputStream(rawBytes);
            try
            {
                switch ((AttrType)attr.Id)
                {
                    case AttrType.AttrName:
                        var enemyName = reader.ReadString();
                        _storage.SetPlayerName(enemyUid, enemyName);
                        _logger?.LogInformation("Found monster name {EnemyName} for uuid {EnemyUid}", enemyName, enemyUid);
                        break;
                    case AttrType.AttrId:
                        var templateId = reader.ReadInt32();
                        _storage.SetNpcTemplateId(enemyUid, templateId);
                        _logger?.LogInformation("Set enemy {Uid} template id{templateId}", enemyUid, templateId);
                        break;
                    case AttrType.AttrHp:
                        var enemyHp = reader.ReadInt32();
                        _storage.SetPlayerHP(enemyUid, enemyHp);
                        break;
                    case AttrType.AttrMaxHp:
                        var enemyMaxHp = reader.ReadInt32();
                        _storage.SetPlayerMaxHP(enemyUid, enemyMaxHp);
                        break;
                }
            }
            catch (InvalidProtocolBufferException ex)
            {
                _logger?.LogWarning(ex, "Failed to decode attr {AttrId} for enemy {EnemyUid}", attr.Id, enemyUid);
            }
        }
    }
}
