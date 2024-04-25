using AppBlazor.Shared.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AppBlazor.Server
{
    public class AplicationDbContext : IdentityDbContext
    {
        public AplicationDbContext(DbContextOptions options) : base(options)//aqui se pasan las configuracion de conexion
        {
        }

        //API fluente
        //realizar la onfiguracion del modelo
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);//NO BORRAR

            //conf la entidad tal, tiene llave (has) -->Llave primaria compuesta para la entidad
            modelBuilder.Entity<GeneroPelicula>().HasKey(g => new { g.GeneroId, g.PeliculaId });//aqui se pueden conf las llaves (compuestas tambien ee)

            modelBuilder.Entity<ActorPelicula>().HasKey(a => new { a.PeliculaId, a.ActorId });
        }

        //configurar que una clase(entidad) sea una entidad
        //dbSet--se quiere crear una tabla a partir de la clase indicada, nombre de la tabla, a travez de un sert se crea dbSet
        public DbSet<Genero> Generos => Set<Genero>();
        public DbSet<Actor> Actores => Set<Actor>();
        public DbSet<Pelicula> Peliculas => Set<Pelicula>();
        public DbSet<VotoPelicula> VotosPeliculas => Set<VotoPelicula>();
        public DbSet<GeneroPelicula> GenerosPeliculas => Set<GeneroPelicula>();//para acceder directo a esta entidad
        public DbSet<ActorPelicula> ActoresPelicula => Set<ActorPelicula>();//para acceder directo a esta entidad



    }
}
