﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PostgreSqlEfAudit.Api.Data;

#nullable disable

namespace PostgreSqlEfAudit.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240124114157_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PostgreSqlEfAudit.Api.Entities.MyEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("MyEntities");
                });

            modelBuilder.Entity("PostgreSqlEfAudit.Api.Entities.MyEntity", b =>
                {
                    b.OwnsOne("PostgreSqlEfAudit.Api.Entities.MyProperties", "MyProperties", b1 =>
                        {
                            b1.Property<Guid>("MyEntityId")
                                .HasColumnType("uuid");

                            b1.HasKey("MyEntityId");

                            b1.ToTable("MyEntities");

                            b1.ToJson("MyProperties");

                            b1.WithOwner()
                                .HasForeignKey("MyEntityId");

                            b1.OwnsMany("PostgreSqlEfAudit.Api.Entities.MyNestedProperty", "Properties", b2 =>
                                {
                                    b2.Property<Guid>("MyPropertiesMyEntityId")
                                        .HasColumnType("uuid");

                                    b2.Property<int>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("integer");

                                    b2.Property<string>("Value")
                                        .IsRequired()
                                        .HasColumnType("text");

                                    b2.HasKey("MyPropertiesMyEntityId", "Id");

                                    b2.ToTable("MyEntities");

                                    b2.WithOwner()
                                        .HasForeignKey("MyPropertiesMyEntityId");
                                });

                            b1.Navigation("Properties");
                        });

                    b.Navigation("MyProperties")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
