using Amusing.ContactRepair;
namespace Amusing.Mijn.Tests;
public class ContactRepairPolicyTests
{
 static ContactLink L(uint id,string role,string? email=null,bool active=true)=>new(id,role,email??$"person{id}@example.invalid",active);
 [Fact] public void HighestIdStaysFirstAndOtherUniqueEmailsBecomeSecond()
 {
  var before=new[]{L(1,"contactpersoon1"),L(2,"contactpersoon1"),L(3,"contactpersoon2")};
  var after=ContactPolicy.Normalize(before);
  Assert.Contains(L(2,"contactpersoon1"),after);Assert.Contains(L(1,"contactpersoon2"),after);Assert.Contains(before[2],after);
  Assert.Equal(after,ContactPolicy.Normalize(after));
 }
 [Fact] public void SameEmailFirstContactsKeepHighestIdWithoutLosingEmail()
 {
  var after=ContactPolicy.Normalize([L(1,"contactpersoon1"," SAME@example.invalid "),L(2,"contactpersoon1","same@example.invalid")]);
  Assert.Single(after);Assert.Equal(2u,after[0].PersonId);
 }
 [Fact] public void ExistingSecondContactWinsOverFirstWithSameEmail()
 {
  var after=ContactPolicy.Normalize([L(9,"contactpersoon1","same@example.invalid"),L(2,"contactpersoon1"),L(1,"contactpersoon2","same@example.invalid")]);
  Assert.Contains(L(2,"contactpersoon1"),after);Assert.Contains(L(1,"contactpersoon2","same@example.invalid"),after);Assert.DoesNotContain(after,r=>r.PersonId==9);
 }
 [Fact] public void AllFirstContactsShadowedBySecondContactsAreBlocked()
 {
  Assert.Throws<InvalidOperationException>(()=>ContactPolicy.Normalize([L(1,"contactpersoon1"),L(2,"contactpersoon1"),L(3,"contactpersoon2","person1@example.invalid"),L(4,"contactpersoon2","person2@example.invalid")]));
 }
 [Fact] public void InactiveNewestDuplicateCannotReplaceReachableActiveContact()
 {
  Assert.Throws<InvalidOperationException>(()=>ContactPolicy.Normalize([L(1,"contactpersoon1","same@example.invalid"),L(2,"contactpersoon1","same@example.invalid",false)]));
 }
 [Fact] public void EveryUniqueEmailAndUnrelatedRolesSurvives()
 {
  var before=new[]{L(1,"contactpersoon1"),L(2,"contactpersoon1"),L(3,"contactpersoon2"),L(4,"penningmeester"),L(1,"zanger")};
  var after=ContactPolicy.Normalize(before);
  Assert.Contains(before[3],after);Assert.Contains(before[4],after);
  Assert.Equal(new uint[]{1,2,3},after.Where(r=>ContactPolicy.IsContact(r.Role)).Select(r=>r.PersonId).Order().ToArray());
 }
 [Fact] public void DifferentSecondContactsRemainAndSameEmailSecondsKeepHighestId()
 {
  var after=ContactPolicy.Normalize([L(1,"contactpersoon1"),L(2,"contactpersoon2","same@example.invalid"),L(3,"contactpersoon2","same@example.invalid"),L(4,"contactpersoon2")]);
  Assert.Equal(3,after.Count);Assert.DoesNotContain(after,r=>r.PersonId==2);
 }
}
