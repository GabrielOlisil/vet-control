using System;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public class CartaoVacinaConfiguration : IEntityTypeConfiguration<CartaoVacina>
{
    public void Configure(EntityTypeBuilder<CartaoVacina> builder)
    {
        return;
    }
}
