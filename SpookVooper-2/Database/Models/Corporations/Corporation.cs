using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Valour.Api.Models;

namespace SV2.Database.Models.Corporations;

public class Corporation
{
    [Key]
    public long Id { get; set; }

    public long GroupId { get; set; }

    [NotMapped]
    public Group Group => DBCache.Get<Group>(GroupId)!;

    public async Task ExecuteDividends(VooperDB dbctx)
    {
        var group = DBCache.Get<Group>(GroupId)!;
        var shares = await dbctx.CorporationShares.Where(x => x.CorporationId == Id).ToListAsync();
        foreach (var share in shares)
        {
            if (share.ShareClass.DividendRate > 0.0m)
            {
                var amount = share.ShareClass.DividendRate * share.Amount / 30 / 24;
                var tran = new SVTransaction(group, BaseEntity.Find(share.EntityId), amount, TransactionType.DividendPayment, $"Dividend Pay for {share.Amount} Class {share.ShareClass.ClassName} shares of Corporation {group.Name}");
                tran.NonAsyncExecute(true);
            }
        }
    }

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
