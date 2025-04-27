using Forum.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forum.Data
{
    public class ForumDbContext : DbContext
    {
        public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<DeletedPost> DeletedPosts { get; set; }
        public DbSet<DeletedComment> DeletedComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                // Уникальный индекс для имени пользователя
                entity.HasIndex(e => e.Username).IsUnique();

                // Связь с Ролью
                entity.HasOne(d => d.Role)
                      .WithMany(p => p.Users)
                      .HasForeignKey(d => d.RoleId)
                      // Запретить удаление роли, если есть пользователи
                      .OnDelete(DeleteBehavior.Restrict); 

                // Связь с Записями
                entity.HasMany(u => u.Posts)
                      .WithOne(p => p.User)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade); 

                // Связь с Комментариями
                entity.HasMany(u => u.Comments)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            modelBuilder.Entity<Post>(entity =>
            {
                // Связь с Комментариями
                entity.HasMany(p => p.Comments)
                      .WithOne(c => c.Post)
                      .HasForeignKey(c => c.PostId)
                      // Удалить комментарии при удалении поста
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                // Индекс для быстрого поиска комментариев к посту
                entity.HasIndex(c => c.PostId);
            });


            // Начальные данные для Ролей
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );
        }
    }
}
