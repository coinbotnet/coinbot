﻿// <auto-generated />
using System;
using Coinbot.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Coinbot.SQLite.Migrations
{
    [DbContext(typeof(CoinbotContext))]
    [Migration("20190629145814_AddedIndexes")]
    partial class AddedIndexes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity("Coinbot.SQLite.Models.Order", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BaseCoin");

                    b.Property<bool>("Bought");

                    b.Property<double>("BoughtFor");

                    b.Property<string>("BuyId");

                    b.Property<double>("ChangeToSell");

                    b.Property<DateTime>("InsertedAt");

                    b.Property<double>("QuantityBought");

                    b.Property<double>("QuantitySold");

                    b.Property<string>("SellId");

                    b.Property<bool>("SellOrderPlaced");

                    b.Property<string>("SessionId");

                    b.Property<bool>("Sold");

                    b.Property<double?>("SoldFor");

                    b.Property<double>("Stack");

                    b.Property<string>("Stock");

                    b.Property<string>("TargetCoin");

                    b.Property<double>("ToSellFor");

                    b.Property<DateTime?>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("SellId", "BuyId", "Stock");

                    b.ToTable("Orders");
                });
#pragma warning restore 612, 618
        }
    }
}
