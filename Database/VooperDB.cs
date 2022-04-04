using Microsoft.EntityFrameworkCore;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Items;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Forums;

/*  Valour - A free and secure chat client
 *  Copyright (C) 2021 Vooper Media LLC
 *  This program is subject to the GNU Affero General Public license
 *  A copy of the license should be included - if not, see <http://www.gnu.org/licenses/>
 */

namespace SV2.Database;

public class VooperDB : DbContext
{

    public static VooperDB Instance = new VooperDB(DBOptions);

    public static string ConnectionString = $"Host={DBConfig.instance.Host};Database={DBConfig.instance.Database};Username={DBConfig.instance.Username};Pwd={DBConfig.instance.Password}";

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(ConnectionString, options => { 
            options.EnableRetryOnFailure(); 
        });
        options.UseLowerCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //modelBuilder.HasCharSet(CharSet.Utf8Mb4);
    }

    /// <summary>
    /// This is only here to fulfill the need of the constructor.
    /// It does literally nothing at all.
    /// </summary>
    public static DbContextOptions DBOptions = new DbContextOptionsBuilder().UseNpgsql(ConnectionString, options => {
        options.EnableRetryOnFailure();
    }).Options;


    /// <summary>
    /// Table for SV2 users
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Table for SV2 groups
    /// </summary>
    public DbSet<Group> Groups { get; set; }

    public DbSet<TaxPolicy> TaxPolicies { get; set; }
    public DbSet<ForumPost> ForumPosts { get; set; }
    public DbSet<TradeItemDefinition> TradeItemDefinitions {get; set; }
    public DbSet<TradeItem> TradeItems { get; set; }
    public DbSet<Factory> Factories { get; set; }
    public DbSet<Recipe> Recipes { get; set; }

    public VooperDB(DbContextOptions options)
    {
            
    }
}
