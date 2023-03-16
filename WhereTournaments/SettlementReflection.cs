using System.Reflection;

using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace WhereTournaments
{
    public static class SettlementReflection
    {
        static MethodInfo addMobileParty = typeof(Settlement).GetMethod(nameof(AddMobileParty), BindingFlags.Instance | BindingFlags.NonPublic);
        public static void AddMobileParty(this Settlement settlement, MobileParty mobileParty)
        {
            addMobileParty.Invoke(settlement, new object[] { mobileParty });
        }
        //internal void AddMobileParty(MobileParty mobileParty)
        //{
        //    if (!_partiesCache.Contains(mobileParty))
        //    {
        //        _partiesCache.Add(mobileParty);
        //        if (mobileParty.IsLordParty)
        //        {
        //            _numberOfLordPartiesAt++;
        //        }
        //    }
        //    else
        //    {
        //        Debug.FailedAssert("mobileParty is already in mobileParties List!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Settlements\\Settlement.cs", "AddMobileParty", 657);
        //    }
        //}

        static MethodInfo removeMobileParty = typeof(Settlement).GetMethod(nameof(RemoveMobileParty), BindingFlags.Instance | BindingFlags.NonPublic);
        public static void RemoveMobileParty(this Settlement settlement, MobileParty mobileParty)
        {
            removeMobileParty.Invoke(settlement, new object[] { mobileParty });
        }
        //internal void RemoveMobileParty(MobileParty mobileParty)
        //{
        //    if (_partiesCache.Contains(mobileParty))
        //    {
        //        _partiesCache.Remove(mobileParty);
        //        if (mobileParty.IsLordParty)
        //        {
        //            _numberOfLordPartiesAt--;
        //        }
        //    }
        //    else
        //    {
        //        Debug.FailedAssert("mobileParty is not in mobileParties List", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Settlements\\Settlement.cs", "RemoveMobileParty", 676);
        //    }
        //}
    }
}