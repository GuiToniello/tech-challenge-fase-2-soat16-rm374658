using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Oficina.Domain.Features.Veiculos;
using TechChallenge.Oficina.Domain.Features.Veiculos.VOs;

namespace TechChallenge.Oficina.Infra.Data.Features.Veiculos;

public sealed class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("veiculos");

        builder.HasKey(veiculo => veiculo.Id);

        builder.Property(veiculo => veiculo.Placa)
            .HasColumnName("placa")
            .HasMaxLength(7)
            .IsRequired()
            .HasConversion(
                placa => placa.Valor,
                valor => PlacaMercosul.Criar(valor));

        builder.HasIndex(veiculo => veiculo.Placa)
            .IsUnique();

        builder.Property(veiculo => veiculo.Marca)
            .HasColumnName("marca")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(veiculo => veiculo.Modelo)
            .HasColumnName("modelo")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(veiculo => veiculo.Ano)
            .HasColumnName("ano")
            .IsRequired();

        builder.Property(veiculo => veiculo.Renavam)
            .HasColumnName("renavam")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(veiculo => veiculo.ClienteId)
            .HasColumnName("cliente_id")
            .IsRequired();

        builder.HasOne<TechChallenge.Oficina.Domain.Features.Clientes.Cliente>()
            .WithMany()
            .HasForeignKey(veiculo => veiculo.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
