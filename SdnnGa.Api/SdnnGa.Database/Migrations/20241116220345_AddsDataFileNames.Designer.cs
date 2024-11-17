﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SdnnGa.Database;

#nullable disable

namespace SdnnGa.Database.Migrations
{
    [DbContext(typeof(ApiDbContext))]
    [Migration("20241116220345_AddsDataFileNames")]
    partial class AddsDataFileNames
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SdnnGa.Database.Models.DbEpoch", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("EpochNo")
                        .HasColumnType("integer");

                    b.Property<bool>("IsTrained")
                        .HasColumnType("boolean");

                    b.Property<int>("ModelCount")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("RecCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RecModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SessionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.ToTable("Epochs");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbFitConfig", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<float>("Alpha")
                        .HasColumnType("real");

                    b.Property<int>("FitMethod")
                        .HasColumnType("integer");

                    b.Property<int>("LossFunc")
                        .HasColumnType("integer");

                    b.Property<int>("MaxEpoches")
                        .HasColumnType("integer");

                    b.Property<int[]>("MetricFuncs")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("RecCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RecModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SessionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("XDataFileName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("YDataFileName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.HasIndex("SessionId")
                        .IsUnique();

                    b.ToTable("FitConfigs");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbGeneticConfig", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("MaxEpoches")
                        .HasColumnType("integer");

                    b.Property<float>("MutationCof")
                        .HasColumnType("real");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("RecCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RecModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SessionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SessionId")
                        .IsUnique();

                    b.ToTable("GeneticConfigs");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbNeuralNetworkModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<float>("AccuracyValue")
                        .HasColumnType("real");

                    b.Property<string>("EpocheId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsTrained")
                        .HasColumnType("boolean");

                    b.Property<int>("Loss")
                        .HasColumnType("integer");

                    b.Property<float>("LossValue")
                        .HasColumnType("real");

                    b.Property<int[]>("Metric")
                        .HasColumnType("integer[]");

                    b.Property<string>("ModelConfigFileName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("RecCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RecModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SessionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("WeightsFileName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.HasIndex("EpocheId");

                    b.HasIndex("SessionId");

                    b.ToTable("NeuralNetworkModel");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbSession", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("RecCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RecModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("XTrainFileName")
                        .HasColumnType("text");

                    b.Property<string>("YTrainFileName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbEpoch", b =>
                {
                    b.HasOne("SdnnGa.Database.Models.DbSession", "Session")
                        .WithMany("Epochs")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Session");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbFitConfig", b =>
                {
                    b.HasOne("SdnnGa.Database.Models.DbSession", "Session")
                        .WithOne("FitConfig")
                        .HasForeignKey("SdnnGa.Database.Models.DbFitConfig", "SessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Session");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbGeneticConfig", b =>
                {
                    b.HasOne("SdnnGa.Database.Models.DbSession", "Session")
                        .WithOne("GeneticConfig")
                        .HasForeignKey("SdnnGa.Database.Models.DbGeneticConfig", "SessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Session");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbNeuralNetworkModel", b =>
                {
                    b.HasOne("SdnnGa.Database.Models.DbEpoch", "Epoch")
                        .WithMany("NeuralNetworkModel")
                        .HasForeignKey("EpocheId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SdnnGa.Database.Models.DbSession", "Session")
                        .WithMany("NeuralNetworkModels")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Epoch");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbEpoch", b =>
                {
                    b.Navigation("NeuralNetworkModel");
                });

            modelBuilder.Entity("SdnnGa.Database.Models.DbSession", b =>
                {
                    b.Navigation("Epochs");

                    b.Navigation("FitConfig");

                    b.Navigation("GeneticConfig");

                    b.Navigation("NeuralNetworkModels");
                });
#pragma warning restore 612, 618
        }
    }
}
