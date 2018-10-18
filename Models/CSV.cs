using System;
using CsvHelper.Configuration;
//using FluentNHibernate.Mapping;

namespace SanctionsApi.Models {
    public class CSV{
        public string Name6  { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Name5 { get; set; }
        public string Title { get; set; }
        public string DOB { get; set; }
        public string TownOfBirth { get; set; }
        public string CountryOfBirth { get; set; }
        public string Nationality { get; set; }
        public string PassportDetails { get; set; }
        public string NINumber { get; set; }
        public string Position { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Address5 { get; set; }
        public string Address6 { get; set; }
        public string PostZipCode { get; set; }
        public string Country { get; set; }
        public string OtherInformation { get; set; }
        public string GroupType { get; set; }
        public string AliasType { get; set; }
        public string Regime { get; set; }
        public string ListedOn { get; set; }
        public string LastUpdated { get; set; }
        public string GroupID { get; set; }
        public string recordnumber {get; set;}
    }

    public sealed class CSVMap : ClassMap<CSV> {
        public CSVMap()
        {
            //Map( m => m.Id )).Name( "id" )).Name( "" );
            
            Map( m => m.Name1).Name( "Name 1" );
            Map( m => m.Name6).Name( "Name 6" );
            Map( m => m.Name2).Name( "Name 2" );
            Map( m => m.Name3).Name( "Name 3" );
            Map( m => m.Name4).Name( "Name 4" );
            Map( m => m.Name5).Name( "Name 5" );
            Map( m => m.Title).Name( "Title" );
            Map( m => m.DOB).Name( "DOB" );
            Map( m => m.TownOfBirth).Name( "Town of Birth" );
            Map( m => m.CountryOfBirth).Name( "Country of Birth" );
            Map( m => m.Nationality).Name( "Nationality" );
            Map( m => m.PassportDetails).Name( "Passport Details" );
            Map( m => m.NINumber).Name( "NI Number" );
            Map( m => m.Position).Name( "Position" );
            Map( m => m.Address1).Name( "Address 1" );
            Map( m => m.Address2).Name( "Address 2" );
            Map( m => m.Address3).Name( "Address 3" );
            Map( m => m.Address4).Name( "Address 4" );
            Map( m => m.Address5).Name( "Address 5" );
            Map( m => m.Address6).Name( "Address 6" );
            Map( m => m.PostZipCode).Name( "Post/Zip Code" );
            Map( m => m.Country).Name( "Country" );
            Map( m => m.OtherInformation).Name( "Other Information" );
            Map( m => m.GroupType).Name( "Group Type" );
            Map( m => m.AliasType).Name( "Alias Type" );
            Map( m => m.Regime).Name( "Regime" );
            Map( m => m.ListedOn).Name( "Listed On" );
            Map( m => m.LastUpdated).Name( "Last Updated" );
            Map( m => m.GroupID).Name( "Group ID" );
        }
    }    
}