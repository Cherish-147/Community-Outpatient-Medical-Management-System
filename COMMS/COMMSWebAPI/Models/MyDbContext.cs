using Microsoft.EntityFrameworkCore;

namespace COMMSWebAPI.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().ToTable("Users");
        }

        //场景加                   ToTable不加         ToTable
        //表名与类名一致           可加可不加          正常
        //表名与类名不一致         必须加              会报错
        //需要映射视图或特殊表     必须加              无法映射

        public DbSet<Departments> Departments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payments>  Payments  { get; set; }
    }
}
