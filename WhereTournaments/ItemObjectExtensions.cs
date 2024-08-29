using System.Collections.Generic;

using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace WhereTournaments.SelectTournamentPrizes
{
    public static class ItemObjectExtensions
    {
        public static TextObject ToToolTipTextObject(this EquipmentElement equipmentElement)
        {
            return ToToolTipTextObject(equipmentElement.Item);
        }

        public static TextObject ToToolTipTextObject(this ItemObject item)
        {
            string mountdesc = "\nCharge: {CHARGE}\n Speed: {SPEED}\nManeuver: {MANEUVER}\nHealth: {HEALTH}";

            string desc = "{NAME}";
            Dictionary<string, string> keyValues = new Dictionary<string, string>
            {
                { "NAME", item.Name.ToString() }
            };

            if (item.Tier != null)
            {
                desc += "\nTier: {TIER}\n";
                keyValues.Add(
                    "TIER",
                    ((int) item.Tier).ToString()
                );
            }
            else
            {
                desc += "\n\n";
            }

            if (item.Weight > 0)
            {
                desc += "\nWeight: {WEIGHT}";
                keyValues.Add(
                    "WEIGHT",
                    item.Weight.ToString()
                );
            }

            if (item.IsCraftedWeapon)
            {
                if (IsOneHanded(item))
                {
                    desc += "\nOne Handed";
                }
                if (IsTwoHanded(item))
                {
                    desc += "\nTwo Handed";
                }
                //desc += "{WEAPON_TYPE}\n";
                //keyValues.Add("WEAPON_TYPE", item.PrimaryWeapon.WeaponClass.ToString());
                if (item.PrimaryWeapon.SwingSpeed > 0)
                {
                    desc += "\nSwing Speed: {SWING_SPEED}";
                    keyValues.Add(
                        "SWING_SPEED",
                        item.WeaponComponent.PrimaryWeapon.SwingSpeed.ToString()
                    );
                }
                if (item.PrimaryWeapon.SwingSpeed > 0)
                {
                    desc += "\nSwing Damage: {SWING_DAMAGE}";
                    keyValues.Add(
                        "SWING_DAMAGE",
                        item.WeaponComponent.PrimaryWeapon.SwingDamage.ToString()
                            + item.PrimaryWeapon.SwingDamageType.ToString().Substring(0, 1)
                    );
                }
                if (item.PrimaryWeapon.ThrustSpeed > 0)
                {
                    desc += "\nThrust Speed: {THRUST_SPEED}";
                    keyValues.Add(
                        "THRUST_SPEED",
                        item.WeaponComponent.PrimaryWeapon.ThrustSpeed.ToString()
                    );
                }
                if (item.PrimaryWeapon.ThrustSpeed > 0)
                {
                    desc += "\nThrust Damage: {THRUST_DAMAGE}";
                    keyValues.Add(
                        "THRUST_DAMAGE",
                        item.WeaponComponent.PrimaryWeapon.ThrustDamage.ToString()
                            + item.PrimaryWeapon.ThrustDamageType.ToString().Substring(0, 1)
                    );
                }
                if (item.PrimaryWeapon.WeaponLength > 0)
                {
                    desc += "\nLength: {LENGTH}";
                    keyValues.Add(
                        "LENGTH",
                        item.WeaponComponent.PrimaryWeapon.WeaponLength.ToString()
                    );
                }

                if (IsWeaponCouchable(item))
                {
                    desc += "\nCouchable";
                }

            }
            if (
                item.ItemType == ItemObject.ItemTypeEnum.Bow
                || item.ItemType == ItemObject.ItemTypeEnum.Crossbow
            )
            {
                if (item.PrimaryWeapon.ReloadPhaseCount > 0)
                {
                    desc += "\nReload: {Missile_RELOAD}";
                    keyValues.Add(
                        "Missile_RELOAD",
                        item.WeaponComponent.PrimaryWeapon.ReloadPhaseCount.ToString()
                    );
                }

                if (item.PrimaryWeapon.MissileSpeed > 0)
                {
                    desc += "\nMissile Speed: {Missile_SPEED}";
                    keyValues.Add(
                        "Missile_SPEED",
                        item.WeaponComponent.PrimaryWeapon.MissileSpeed.ToString()
                    );
                }
                if (item.PrimaryWeapon.MissileDamage > 0)
                {
                    desc += "\nMissile Damage: {Missile_DAMAGE}";
                    keyValues.Add(
                        "Missile_DAMAGE",
                        item.WeaponComponent.PrimaryWeapon.MissileDamage.ToString()
                    );
                }
            }
            if (item.ItemType == ItemObject.ItemTypeEnum.Shield)
            {
                desc += "\nHP: {HP}";
                keyValues.Add(
                    "HP",
                    item.WeaponComponent.PrimaryWeapon.GetModifiedMaximumHitPoints(null).ToString()
                );
            }
            if (item.ArmorComponent != null)
            {
                if (item.ArmorComponent.HeadArmor > 0)
                {
                    desc += "\nHead Armor: {HEAD_ARMOR}";
                    keyValues.Add("HEAD_ARMOR", item.ArmorComponent.HeadArmor.ToString());
                }
                if (item.ArmorComponent.BodyArmor > 0)
                {
                    desc += "\nBody Armor: {BODY_ARMOR}";
                    keyValues.Add("BODY_ARMOR", item.ArmorComponent.BodyArmor.ToString());
                }
                if (item.ArmorComponent.ArmArmor > 0)
                {
                    desc += "\nArm Armor: {ARM_ARMOR}";
                    keyValues.Add("ARM_ARMOR", item.ArmorComponent.ArmArmor.ToString());
                }
                if (item.ArmorComponent.LegArmor > 0)
                {
                    desc += "\nLeg Armor: {LEG_ARMOR}";
                    keyValues.Add("LEG_ARMOR", item.ArmorComponent.LegArmor.ToString());
                }
                if (item.ArmorComponent.ChargeBonus > 0)
                {
                    desc += "\nLeg Armor: {CHARGE_BONUS}";
                    keyValues.Add("CHARGE_BONUS", item.ArmorComponent.ChargeBonus.ToString());
                }
            }
            else if (item.ItemType == ItemObject.ItemTypeEnum.HorseHarness)
            {
                desc += "\nBody Armor: {BODY_ARMOR}";
                keyValues.Add("BODY_ARMOR", item.GetMountBodyArmor().ToString());
            }
            if (item.IsMountable)
            {
                desc += mountdesc;

                keyValues.Add("CHARGE", item.HorseComponent.ChargeDamage.ToString());
                keyValues.Add("SPEED", item.HorseComponent.Speed.ToString());
                keyValues.Add("MANEUVER", item.HorseComponent.Maneuver.ToString());
                keyValues.Add("HEALTH", item.HorseComponent.HitPoints.ToString());
            }
            desc += "\n\nValue: {VALUE}";
            keyValues.Add("VALUE", item.Value.ToString());

            TextObject toolTip = new TextObject(desc);
            foreach (var k in keyValues.Keys)
            {
                toolTip.SetTextVariable(k, keyValues[k]);
            }
            return toolTip;
        }

        internal static bool IsWeaponCouchable(ItemObject weapon)
        {
            bool flag = false;
            using (IEnumerator<WeaponComponentData> enumerator = weapon.Weapons.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!MBItem.GetItemIsPassiveUsage(enumerator.Current.ItemUsage))
                    {
                        continue;
                    }
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        internal static bool IsOneHanded(ItemObject weapon)
        {
            try
            {
                if (weapon != null
                    && weapon.WeaponComponent != null
                    && weapon.WeaponComponent.PrimaryWeapon != null)
                {
                    switch (weapon.WeaponComponent.PrimaryWeapon.WeaponClass)
                    {
                        case WeaponClass.OneHandedAxe:
                        case WeaponClass.OneHandedSword:
                        case WeaponClass.OneHandedPolearm:
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        internal static bool IsTwoHanded(ItemObject weapon)
        {
            try
            {
                if (weapon != null
                    && weapon.WeaponComponent != null
                    && weapon.WeaponComponent.PrimaryWeapon != null)
                {
                    switch (weapon.WeaponComponent.PrimaryWeapon.WeaponClass)
                    {
                        case WeaponClass.TwoHandedAxe:
                        case WeaponClass.TwoHandedMace:
                        case WeaponClass.TwoHandedPolearm:
                        case WeaponClass.TwoHandedSword:
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }


        internal static int GetMountBodyArmor(this ItemObject Item)
        {
            int bodyArmor = 0;
            if (Item.HasArmorComponent)
            {
                ArmorComponent armorComponent = Item.ArmorComponent;
                bodyArmor = (Item.ItemType == ItemObject.ItemTypeEnum.HorseHarness ? armorComponent.BodyArmor : 0);
            }
            else if (Item.WeaponComponent != null)
            {
                bodyArmor = Item.WeaponComponent.PrimaryWeapon.BodyArmor;
            }
            if (bodyArmor <= 0)
            {
                return 0;
            }
            return bodyArmor;
        }

    }
}