using EventAuthServer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventAuthServer.ModelBuilderExtension
{
    public class MenuDetails : IEntityTypeConfiguration<MenuModel>
    {
        public void Configure(EntityTypeBuilder<MenuModel> builder)
        {
            builder.ToTable("Menu").HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
              .WithMany(y => y.Children)
              .HasForeignKey(a => a.ParentId);
        }
    }
   
}
