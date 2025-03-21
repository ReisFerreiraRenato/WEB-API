﻿using Microsoft.EntityFrameworkCore;

namespace WEB_API.Models
{
    public class ProdutosUsuariosContext: DbContext
    {
        public ProdutosUsuariosContext(DbContextOptions<ProdutosUsuariosContext> options) : base(options)
        {

        }

        public DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<Produto> Produtos { get; set; }
        public DbSet<LogErro> LogsErros { get; set; }
    }
}
