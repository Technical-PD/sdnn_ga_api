using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdnnGa.Model.Database.Models;

public class BaseModel
{
    [Key]
    [Required]
    public string Id { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime RecCreated { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime RecModified { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; }
}