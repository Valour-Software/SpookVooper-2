using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Corporations;

public class Corporation
{
    [Key]
    public long Id { get; set; }

    public long GroupId { get; set; }

    [NotMapped]
    public Group Group => DBCache.Get<Group>(GroupId)!;

    public void CreateFromGroup(Group group, VooperDB dbctx)
    {
        group.GroupType = GroupTypes.Corporation;
        Id = IdManagers.GeneralIdGenerator.Generate();
        GroupId = group.Id;
        var shareclass = new CorporationShareClass()
        {
            Id = IdManagers.GeneralIdGenerator.Generate(),
            CorporationId = Id,
            ClassName = ShareClassName.A,
            ClassType = ShareClassType.Common,
            VotingPower = 1.00m,
            DividendRate = 0.00m
        };
        DBCache.AddNew(shareclass.Id, shareclass);

        var share = new CorporationShare()
        {
            Id = IdManagers.GeneralIdGenerator.Generate(),
            EntityId = group.OwnerId,
            CorporationId = Id,
            ShareClassId = shareclass.Id,
            Amount = 100_000
        };
        DBCache.AddNew(share.Id, share);

        DBCache.AddNew(Id, this);
    }
}
