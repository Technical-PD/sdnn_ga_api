using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdnnGa.Database.Models;

public class BaseModel
{
    [Key]
    [Required]
    public string Id { get; set; }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(TypeName = "timestamp")]
    public DateTime RecCreated { get; set; }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column(TypeName = "timestamp")]
    public DateTime RecModified { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; }
}