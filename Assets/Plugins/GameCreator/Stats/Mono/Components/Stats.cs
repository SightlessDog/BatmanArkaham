namespace GameCreator.Stats
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;

    [AddComponentMenu("Game Creator/Stats/Stats")]
    public class Stats : GlobalID, IGameSave
    {
        [Serializable]
        public class OverrideStatData
        {
            public string statUniqueID = "";
            public bool overrideValue = false;
            public float baseValue;

            public bool overrideFormula = false;
            public FormulaAsset formula;
        }

        [Serializable]
        public class SerialStat
        {
            public string statUniqueID = "";
            public float baseValue = 0.0f;

            public SerialStat(string uniqueID, float baseValue)
            {
                this.statUniqueID = uniqueID;
                this.baseValue = baseValue;
            }
        }

        [Serializable]
        public class SerialAttr
        {
            public string statUniqueID = "";
            public float value = 0.0f;

            public SerialAttr(string uniqueID, float value)
            {
                this.statUniqueID = uniqueID;
                this.value = value;
            }
        }

        [Serializable]
        public class SerialStef
        {
            public string stefUniqueID = "";
            public float[] durationStack = new float[0];

            public SerialStef(string uniqueID, float[] timeStack)
            {
                this.stefUniqueID = uniqueID;
                this.durationStack = timeStack;
            }
        }

        [Serializable]
        public class SerialData
        {
            public SerialStat[] stats = new SerialStat[0];
            public SerialAttr[] attrs = new SerialAttr[0];
            public SerialStef[] stefs = new SerialStef[0];
        }

        public class RuntimeStatData
        {
            public int index = 0;
            public float baseValue = 0.0f;

            public FormulaAsset formula;
            public StatAsset statAsset;

            public UnityEvent onChange;
            public List<StatModifier> statsModifiers;

            public RuntimeStatData(int index, StatAsset statAsset)
            {
                this.index = index;

                this.baseValue = statAsset.stat.baseValue;
                this.formula = statAsset.stat.formula;

                this.statAsset = statAsset;
                this.onChange = new UnityEvent();
                this.statsModifiers = new List<StatModifier>();
            }
        }

        public class RuntimeAttrData
        {
            public int index = 0;
            public float value = 0.0f;

            public StatAsset statAsset = null;
            public AttrAsset attrAsset = null;

            public UnityEvent onChange;

            public RuntimeAttrData(int index, float value, AttrAsset attrAsset)
            {
                this.index = index;
                this.value = value;
                this.attrAsset = attrAsset;
                this.statAsset = attrAsset.attribute.stat;
                this.onChange = new UnityEvent();
            }
        }

        public class RuntimeStefData
        {
            public List<StatusEffect.Runtime> listStatus = new List<StatusEffect.Runtime>();
            public StatusEffectAsset statusEffect;

            public RuntimeStefData(StatusEffectAsset statusEffect)
            {
                this.listStatus = new List<StatusEffect.Runtime>();
                this.statusEffect = statusEffect;
            }

            public RuntimeStefData(string uniqueID, float[] timeStack)
            {
                this.listStatus = new List<StatusEffect.Runtime>();
                this.statusEffect = DatabaseStats.Load().GetStatusEffect(uniqueID);

                for (int i = 0; i < timeStack.Length; ++i)
                {
                    this.listStatus.Add(new StatusEffect.Runtime(
                        timeStack[i], this.statusEffect.statusEffect
                    ));
                }
            }
        }

        // EVENTS: --------------------------------------------------------------------------------

        public class EventArgs
        {
            public enum Operation
            {
                Add,
                Remove,
                Change,
            }

            public string name;
            public Operation operation;

            public EventArgs(string name, Operation operation)
            {
                this.name = name;
                this.operation = operation;
            }
        }

        // STATIC AND CONSTANTS: ------------------------------------------------------------------

        private Dictionary<string, string> STATS_TLB = null;
        private Dictionary<string, string> ATTRS_TLB = null;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool saveStats = true;

        public List<OverrideStatData> statsOverrides = new List<OverrideStatData>();

        public Dictionary<string, RuntimeStatData> runtimeStatsData { get; private set; }
        public Dictionary<string, RuntimeAttrData> runtimeAttrsData { get; private set; }
        public Dictionary<string, RuntimeStefData> runtimeStefsData { get; private set; }

        private Action<EventArgs> onChangeStat;
        private Action<EventArgs> onChangeAttr;
        private Action<EventArgs> onChangeStef;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            if (!Application.isPlaying) return;
            this.RequireInit();
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            if (this.saveStats) SaveLoadManager.Instance.Initialize(this);
        }

        private void OnDestroy()
        {
            this.OnDestroyGID();
            if (!Application.isPlaying) return;
            if (this.exitingApplication) return;

            if (this.saveStats) SaveLoadManager.Instance.OnDestroyIGameSave(this);
        }

        // UPDATE METHOD: -------------------------------------------------------------------------

        private void Update()
        {
            if (!Application.isPlaying) return;
            if (this.runtimeStefsData == null) return;

            Dictionary<string, int> removeStatusEffects = new Dictionary<string, int>();
            foreach (KeyValuePair<string, RuntimeStefData> item in this.runtimeStefsData)
            {
                RuntimeStefData data = item.Value;
                if (data.statusEffect.statusEffect.hasDuration)
                {
                    int listStatusIndex = 0;
                    while (listStatusIndex < data.listStatus.Count &&
                           data.listStatus[listStatusIndex].time + data.statusEffect.statusEffect.duration < Time.time)
                    {
                        data.listStatus[listStatusIndex].OnEnd(gameObject);
                        if (!removeStatusEffects.ContainsKey(item.Key))
                        {
                            removeStatusEffects.Add(item.Key, 0);
                        }

                        removeStatusEffects[item.Key] += 1;
                        listStatusIndex += 1;
                    }
                }

                int stackCount = data.listStatus.Count;
                int index = (removeStatusEffects.ContainsKey(item.Key)
                    ? removeStatusEffects[item.Key] : 0
                );

                while (index < stackCount)
                {
                    data.listStatus[index].OnUpdate(gameObject);
                    index += 1;
                }
            }

            foreach (KeyValuePair<string, int> item in removeStatusEffects)
            {
                this.runtimeStefsData[item.Key].listStatus.RemoveRange(0, item.Value);

                if (this.onChangeStef != null)
                {
                    this.onChangeStef.Invoke(new EventArgs(
                        this.runtimeStefsData[item.Key].statusEffect.statusEffect.uniqueName,
                        EventArgs.Operation.Remove
                    ));
                }
            }
        }

        // PUBLIC GET METHODS: --------------------------------------------------------------------

        public float GetStat(string stat, Stats target)
        {
            this.RequireInit();
            RuntimeStatData data = this.GetRuntimeStat(stat);
            float result = 0.0f;
            if (data != null)
            {
                result = data.baseValue;
                if (data.formula)
                {
                    result = data.formula.formula.Calculate(
                        data.baseValue, this, target
                    );
                }

                float sumPercent = 0f;

                for (int i = 0; i < data.statsModifiers.Count; i++)
                {
                    StatModifier statModifier = data.statsModifiers[i];
                    switch (statModifier.type)
                    {
                        case StatModifier.EffectType.Contant:
                            result += statModifier.value;
                            break;

                        case StatModifier.EffectType.Percent:
                            sumPercent += (statModifier.value/100f);
                            if (i + 1 >= data.statsModifiers.Count ||
                                data.statsModifiers[i + 1].type != StatModifier.EffectType.Percent)
                            {
                                result *= (1.0f + sumPercent);
                                sumPercent = 0f;
                            }
                            break;
                    }
                }
            }

            return result;
        }

        public float GetStat(string stat)
        {
            return this.GetStat(stat, null);
        }

        public float GetAttrValue(string attribute, Stats target)
        {
            this.RequireInit();
            RuntimeAttrData data = this.GetRuntimeAttr(attribute);
            return data.value;
        }

        public float GetAttrValue(string attribute)
        {
            return this.GetAttrValue(attribute, null);
        }

        public float GetAttrMaxValue(string attribute, Stats target)
        {
            this.RequireInit();
            RuntimeAttrData data = this.GetRuntimeAttr(attribute);
            return this.GetStat(data.statAsset.stat.uniqueName, target);
        }

        public float GetAttrMaxValue(string attribute)
        {
            return this.GetAttrMaxValue(attribute, null);
        }

        public float GetAttrValuePercent(string attribute, Stats target)
        {
            return this.GetAttrValue(attribute, target) / this.GetAttrMaxValue(attribute, target);
        }

        public float GetAttrValuePercent(string attribute)
        {
            return this.GetAttrValuePercent(attribute, null);
        }

        // PUBLIC SET METHODS: --------------------------------------------------------------------

        public void SetStatBase(string stat, float value, bool informCallbacks = true)
        {
            this.RequireInit();
            RuntimeStatData data = this.GetRuntimeStat(stat);
            data.baseValue = value;
            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeStat != null) this.onChangeStat.Invoke(new EventArgs(
                stat, EventArgs.Operation.Change
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Change
            ));
        }

        public void AddStatBase(string stat, float value, bool informCallbacks = true)
        {
            this.RequireInit();
            RuntimeStatData data = this.GetRuntimeStat(stat);
            data.baseValue += value;

            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeStat != null) this.onChangeStat.Invoke(new EventArgs(
                stat, EventArgs.Operation.Change
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Change
            ));
        }

        public void MultiplyStatBase(string stat, float value, bool informCallbacks = true)
        {
            this.RequireInit();
            RuntimeStatData data = this.GetRuntimeStat(stat);
            data.baseValue *= value;

            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeStat != null) this.onChangeStat.Invoke(new EventArgs(
                stat, EventArgs.Operation.Change
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Change
            ));
        }

        public void SetAttrValue(string attribute, float value, bool informCallbacks = true)
        {
            this.RequireInit();
            RuntimeAttrData data = this.GetRuntimeAttr(attribute);
            data.value = value;
            data.value = Mathf.Clamp(
                data.value,
                data.attrAsset.attribute.minValue,
                this.GetStat(data.statAsset.stat.uniqueName)
            );

            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Change
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                attribute, EventArgs.Operation.Change
            ));
        }

        public void AddAttrValue(string attribute, float value, bool informCallbacks = true)
        {
            this.RequireInit();
            RuntimeAttrData data = this.GetRuntimeAttr(attribute);

            data.value += value;
            data.value = Mathf.Clamp(
                data.value,
                data.attrAsset.attribute.minValue,
                this.GetStat(data.statAsset.stat.uniqueName)
            );

            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Change
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                attribute, EventArgs.Operation.Change
            ));
        }

        public void MultiplyAttrValue(string attribute, float value, bool informCallbacks = true)
        {
            this.RequireInit();
            RuntimeAttrData data = this.GetRuntimeAttr(attribute);
            data.value *= value;
            data.value = Mathf.Clamp(
                data.value,
                data.attrAsset.attribute.minValue,
                this.GetStat(data.statAsset.stat.uniqueName)
            );

            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Change
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                attribute, EventArgs.Operation.Change
            ));
        }

        // PUBLIC UI METHODS: ---------------------------------------------------------------------

        public string GetStatTitle(string statID)
        {
            return this.GetRuntimeStat(statID).statAsset.stat.title.GetText();
        }

        public string GetStatShortName(string statID)
        {
            return this.GetRuntimeStat(statID).statAsset.stat.shortName;
        }

        public string GetStatDescription(string statID)
        {
            return this.GetRuntimeStat(statID).statAsset.stat.description.GetText();
        }

        public Sprite GetStatIcon(string statID)
        {
            return this.GetRuntimeStat(statID).statAsset.stat.icon;
        }

        public Color GetStatColor(string statID)
        {
            return this.GetRuntimeStat(statID).statAsset.stat.color;
        }

        public string GetAttrTitle(string attrID)
        {
            return this.GetRuntimeAttr(attrID).attrAsset.attribute.title.GetText();
        }

        public string GetAttrShortName(string attrID)
        {
            return this.GetRuntimeAttr(attrID).attrAsset.attribute.shortName;
        }

        public string GetAttrDescription(string attrID)
        {
            return this.GetRuntimeAttr(attrID).attrAsset.attribute.description.GetText();
        }

        public Sprite GetAttrIcon(string attrID)
        {
            return this.GetRuntimeAttr(attrID).attrAsset.attribute.icon;
        }

        public Color GetAttrColor(string attrID)
        {
            return this.GetRuntimeAttr(attrID).attrAsset.attribute.color;
        }

        public string GetStatusEffectTitle(string statusEffect)
        {
            return this.runtimeStefsData[statusEffect].statusEffect.statusEffect.title.GetText();
        }

        public string GetStatusEffectDescription(string statusEffect)
        {
            return this.runtimeStefsData[statusEffect].statusEffect.statusEffect.description.GetText();
        }

        public Sprite GetStatusEffectIcon(string statusEffect)
        {
            return this.runtimeStefsData[statusEffect].statusEffect.statusEffect.icon;
        }

        public Color GetStatusEffectColor(string statusEffect)
        {
            return this.runtimeStefsData[statusEffect].statusEffect.statusEffect.color;
        }

        public int GetStatusEffectStack(string statusEffect)
        {
            return this.runtimeStefsData[statusEffect].listStatus.Count;
        }

        // PUBLIC CALLBACK METHODS: ---------------------------------------------------------------

        public void AddOnChangeStat(Action<EventArgs> callback)
        {
            this.onChangeStat += (callback);
        }

        public void RemoveOnChangeStat(Action<EventArgs> callback)
        {
            this.onChangeStat -= callback;
        }

        public void AddOnChangeAttr(Action<EventArgs> callback)
        {
            this.onChangeAttr += callback;
        }

        public void RemoveOnChangeAttr(Action<EventArgs> callback)
        {
            this.onChangeAttr -= callback;
        }

        public void AddOnChangeStef(Action<EventArgs> callback)
        {
            this.onChangeStef += (callback);
        }

        public void RemoveOnChangeStef(Action<EventArgs> callback)
        {
            this.onChangeStef -= callback;
        }

        // PUBLIC STATUS EFFECT METHODS: ----------------------------------------------------------

        public void AddStatusEffect(StatusEffectAsset statusEffect, bool informCallbacks = true)
        {
            if (!statusEffect) return;

            this.RequireInit();
            if (!this.runtimeStefsData.ContainsKey(statusEffect.uniqueID))
            {
                RuntimeStefData data = new RuntimeStefData(statusEffect);
                this.runtimeStefsData.Add(statusEffect.uniqueID, data);
            }

            int currentStack = this.runtimeStefsData[statusEffect.uniqueID].listStatus.Count;
            if (currentStack >= statusEffect.statusEffect.maxStack)
            {
                this.RemoveStatusEffect(statusEffect);
            }

            StatusEffect.Runtime item = new StatusEffect.Runtime(
                Time.time, statusEffect.statusEffect
            );

            this.runtimeStefsData[statusEffect.uniqueID].listStatus.Add(item);
            item.OnStart(gameObject);

            if (informCallbacks && this.onChangeStef != null) this.onChangeStef.Invoke(new EventArgs(
                statusEffect.statusEffect.uniqueName, EventArgs.Operation.Add
            ));
        }

        public void RemoveStatusEffect(StatusEffectAsset statusEffect, bool informCallbacks = true)
        {
            if (!statusEffect) return;

            this.RequireInit();

            if (!this.runtimeStefsData.ContainsKey(statusEffect.uniqueID)) return;
            if (this.runtimeStefsData[statusEffect.uniqueID].listStatus.Count <= 0) return;

            this.runtimeStefsData[statusEffect.uniqueID].listStatus[0].OnEnd(gameObject);
            this.runtimeStefsData[statusEffect.uniqueID].listStatus.RemoveAt(0);

            if (informCallbacks && this.onChangeStef != null) this.onChangeStef.Invoke(new EventArgs(
                statusEffect.statusEffect.uniqueName, EventArgs.Operation.Remove
            ));
        }

        public void RemoveStatusEffect(StatusEffect.Type statusEffectType, bool informCallbacks = true)
        {
            this.RequireInit();

            List<string> candidatesID = new List<string>();
            List<string> candidatesNames = new List<string>();
            foreach (KeyValuePair<string, RuntimeStefData> stef in this.runtimeStefsData)
            {
                if (stef.Value.statusEffect.statusEffect.type == statusEffectType)
                {
                    candidatesID.Add(stef.Key);
                    candidatesNames.Add(stef.Value.statusEffect.statusEffect.uniqueName);
                }
            }

            foreach (string candidate in candidatesID)
            {
                while (this.runtimeStefsData[candidate].listStatus.Count > 0)
                {
                    this.runtimeStefsData[candidate].listStatus[0].OnEnd(gameObject);
                    this.runtimeStefsData[candidate].listStatus.RemoveAt(0);
                }
            }

            if (informCallbacks && this.onChangeStef != null)
            {
                foreach (string candidateName in candidatesNames)
                {
                    this.onChangeStef.Invoke(new EventArgs(
                        candidateName, EventArgs.Operation.Remove
                    ));
                }
            }
        }

        public bool HasStatusEffect(StatusEffectAsset statusEffect, int amount = 1)
        {
            this.RequireInit();
            if (!statusEffect) return false;

            return (
                this.runtimeStefsData.ContainsKey(statusEffect.uniqueID) &&
                this.runtimeStefsData[statusEffect.uniqueID].listStatus.Count >= amount
            );
        }

        public List<string> GetStatusEffects()
        {
            return this.GetStatusEffects(false, StatusEffect.Type.Positive);
        }

        public List<string> GetStatusEffects(StatusEffect.Type type)
        {
            return this.GetStatusEffects(true, type);
        }

        private List<string> GetStatusEffects(bool useType, StatusEffect.Type type)
        {
            this.RequireInit();
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, RuntimeStefData> item in this.runtimeStefsData)
            {
                if (item.Value.listStatus.Count > 0)
                {
                    if (!useType || item.Value.statusEffect.statusEffect.type == type)
                    {
                        result.Add(item.Key);
                    }
                }
            }

            return result;
        }

        // PUBLIC STAT MODIFIER METHODS: ----------------------------------------------------------

        public void AddStatModifier(StatModifier statModifier, bool informCallbacks = true)
        {
            this.RequireInit();
            if (!statModifier.stat)
            {
                Debug.LogError("AddStatModifier: Undefined Stat object");
                return;
            }

            RuntimeStatData data = this.runtimeStatsData[statModifier.stat.uniqueID];

            data.statsModifiers.Add(statModifier);
            data.statsModifiers.Sort(this.SortFunctionStatModifiers);

            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeStat != null) this.onChangeStat.Invoke(new EventArgs(
                statModifier.stat.stat.uniqueName, EventArgs.Operation.Add
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Add
            ));
        }

        public void RemoveStatModifier(StatModifier statModifier, bool informCallbacks = true)
        {
            this.RequireInit();
            if (!statModifier.stat)
            {
                Debug.LogError("RemoveStatModifier: Undefined Stat object");
                return;
            }

            RuntimeStatData data = this.runtimeStatsData[statModifier.stat.uniqueID];

            int statModifiersCount = data.statsModifiers.Count;
            bool statModifierFound = false;
            for (int i = 0; !statModifierFound && i < statModifiersCount; ++i)
            {
                StatModifier element = data.statsModifiers[i];
                statModifierFound = (
                    element.stat == statModifier.stat &&
                    element.type == statModifier.type &&
                    Mathf.Approximately(element.value, statModifier.value)
                );

                if (statModifierFound)
                {
                    data.statsModifiers.RemoveAt(i);
                }
            }

            if (!statModifierFound) return;
            data.statsModifiers.Sort(this.SortFunctionStatModifiers);

            if (data.onChange != null) data.onChange.Invoke();
            if (informCallbacks && this.onChangeStat != null) this.onChangeStat.Invoke(new EventArgs(
                statModifier.stat.stat.uniqueName, EventArgs.Operation.Remove
            ));
            if (informCallbacks && this.onChangeAttr != null) this.onChangeAttr.Invoke(new EventArgs(
                string.Empty, EventArgs.Operation.Remove
            ));
        }

        private int SortFunctionStatModifiers(StatModifier a, StatModifier b)
        {
            if (a.type < b.type) return -1;
            if (a.type > b.type) return 1;
            return 0;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RequireInit(bool forceInit = false)
        {
            if (!forceInit && this.runtimeStatsData != null &&
                this.runtimeAttrsData != null && this.runtimeStefsData != null)
            {
                return;
            }

            this.runtimeStatsData = new Dictionary<string, RuntimeStatData>();
            this.runtimeAttrsData = new Dictionary<string, RuntimeAttrData>();
            this.runtimeStefsData = new Dictionary<string, RuntimeStefData>();

            StatAsset[] originStats = DatabaseStats.Load().GetStatAssets();
            AttrAsset[] originAttrs = DatabaseStats.Load().GetAttrAssets();

            if (STATS_TLB == null)
            {
                STATS_TLB = new Dictionary<string, string>();
                for (int i = 0; i < originStats.Length; ++i)
                {
                    STATS_TLB.Add(
                        originStats[i].stat.uniqueName,
                        originStats[i].uniqueID
                    );
                }
            }

            for (int i = 0; i < originStats.Length; ++i)
            {
                this.runtimeStatsData.Add(
                    originStats[i].uniqueID,
                    new RuntimeStatData(i, originStats[i])
                );
            }

            int statsOverridesCount = this.statsOverrides.Count;
            for (int i = 0; i < statsOverridesCount; ++i)
            {
                string uniqueID = this.statsOverrides[i].statUniqueID;
                if (this.runtimeStatsData.ContainsKey(uniqueID))
                {
                    if (this.statsOverrides[i].overrideValue)
                    {
                        this.runtimeStatsData[uniqueID].baseValue = this.statsOverrides[i].baseValue;
                    }

                    if (this.statsOverrides[i].overrideFormula)
                    {
                        this.runtimeStatsData[uniqueID].formula = this.statsOverrides[i].formula;
                    }
                }
            }

            if (ATTRS_TLB == null)
            {
                ATTRS_TLB = new Dictionary<string, string>();
                for (int i = 0; i < originAttrs.Length; ++i)
                {
                    ATTRS_TLB.Add(
                        originAttrs[i].attribute.uniqueName,
                        originAttrs[i].uniqueID
                    );
                }
            }

            for (int i = 0; i < originAttrs.Length; ++i)
            {
                if (!originAttrs[i].attribute.stat)
                {
                    Debug.LogErrorFormat(
                        "Attribute {0} has no Stat assigned",
                        originAttrs[i].attribute.uniqueName
                    );
                    continue;
                }

                string statName = originAttrs[i].attribute.stat.stat.uniqueName;
                float value = Mathf.Lerp(
                    originAttrs[i].attribute.minValue,
                    this.GetStat(statName),
                    originAttrs[i].attribute.percent
                );

                this.runtimeAttrsData.Add(
                    originAttrs[i].uniqueID,
                    new RuntimeAttrData(i, value, originAttrs[i])
                );

                this.AddOnChangeStatUpdateAttribute(originAttrs[i].uniqueID);
            }
        }

        private void AddOnChangeStatUpdateAttribute(string attrID)
        {
            string statID = this.runtimeAttrsData[attrID].statAsset.uniqueID;
            this.runtimeStatsData[statID].onChange.AddListener(() =>
            {
                this.runtimeAttrsData[attrID].value = Mathf.Clamp(
                    this.runtimeAttrsData[attrID].value,
                    this.runtimeAttrsData[attrID].attrAsset.attribute.minValue,
                    this.GetStat(this.runtimeAttrsData[attrID].statAsset.stat.uniqueName)
                );
            });
        }

        private RuntimeStatData GetRuntimeStat(string stat)
        {
            string statTLB = string.Empty;
            if (!this.STATS_TLB.TryGetValue(stat, out statTLB))
            {
                Debug.LogError("Stat " + stat + " not present in TLB. Did you remove it?");
                return null;
            }

            RuntimeStatData statData;
            if (!this.runtimeStatsData.TryGetValue(statTLB, out statData))
            {
                Debug.LogError("Stat " + stat + " not present in RTD. Did you remove it?");
                return null;
            }

            return statData;
        }

        private RuntimeAttrData GetRuntimeAttr(string attrID)
        {
            string attrTLB = string.Empty;
            if (!this.ATTRS_TLB.TryGetValue(attrID, out attrTLB))
            {
                Debug.LogError("Attribute " + attrID + " not present in TLB. Did you remove it?");
                return null;
            }

            RuntimeAttrData attrData;
            if (!this.runtimeAttrsData.TryGetValue(attrTLB, out attrData))
            {
                Debug.LogError("Attribute " + attrID + " not present in RTD. Did you remove it?");
                return null;
            }

            return attrData;
        }

        // IGAMESAVE: -----------------------------------------------------------------------------

        public string GetUniqueName()
        {
            string uniqueName = string.Format(
                "stats:{0}",
                this.GetID()
            );

            return uniqueName;
        }

        public Type GetSaveDataType()
        {
            return typeof(SerialData);
        }

        public object GetSaveData()
        {
            SerialData stats = new SerialData();
            if (this.runtimeStatsData == null) return stats;

            stats.stats = new SerialStat[this.runtimeStatsData.Count];
            stats.attrs = new SerialAttr[this.runtimeAttrsData.Count];
            stats.stefs = new SerialStef[this.runtimeStefsData.Count];

            int index = 0;
            foreach (KeyValuePair<string, RuntimeStatData> item in this.runtimeStatsData)
            {
                stats.stats[index] = new SerialStat(item.Key, item.Value.baseValue);
                index++;
            }

            index = 0;
            foreach (KeyValuePair<string, RuntimeAttrData> item in this.runtimeAttrsData)
            {
                stats.attrs[index] = new SerialAttr(item.Key, item.Value.value);
                index++;
            }

            index = 0;
            foreach (KeyValuePair<string, RuntimeStefData> item in this.runtimeStefsData)
            {
                float[] durations = new float[item.Value.listStatus.Count];
                for (int i = 0; i < durations.Length; ++i)
                {
                    durations[i] = Time.time - item.Value.listStatus[i].time;
                }

                stats.stefs[index] = new SerialStef(item.Key, durations);
                index++;
            }

            return stats;
        }

        public void ResetData()
        {
            this.RequireInit(true);
        }

        public void OnLoad(object generic)
        {
            if (generic == null) return;
            SerialData stats = (SerialData)generic;

            this.RequireInit();
            for (int i = 0; i < stats.stats.Length; ++i)
            {
                string key = stats.stats[i].statUniqueID;
                float baseValue = stats.stats[i].baseValue;
                this.runtimeStatsData[key].baseValue = baseValue;

                if (this.onChangeStat != null)
                {
                    this.onChangeStat.Invoke(new EventArgs(key, EventArgs.Operation.Change));
                }
            }

            for (int i = 0; i < stats.attrs.Length; ++i)
            {
                string key = stats.attrs[i].statUniqueID;
                float value = stats.attrs[i].value;
                this.runtimeAttrsData[key].value = value;

                if (this.onChangeAttr != null)
                {
                    this.onChangeAttr.Invoke(new EventArgs(key, EventArgs.Operation.Change));
                }
            }

            for (int i = 0; i < stats.stefs.Length; ++i)
            {
                string key = stats.stefs[i].stefUniqueID;
                float[] timeStack = stats.stefs[i].durationStack;
                for (int j = 0; j < timeStack.Length; ++j)
                {
                    timeStack[j] = Time.time - timeStack[j];
                }

                RuntimeStefData stefData = new RuntimeStefData(key, timeStack);
                this.runtimeStefsData.Add(key, stefData);

                if (this.onChangeStef != null)
                {
                    this.onChangeStef.Invoke(new EventArgs(key, EventArgs.Operation.Change));
                }

                for (int j = 0; j < timeStack.Length; ++j)
                {
                    stefData.listStatus[j].OnStart(gameObject);
                }
            }
        }
    }
}