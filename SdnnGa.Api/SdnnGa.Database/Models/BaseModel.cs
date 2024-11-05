using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace SdnnGa.Database.Models;

public class BaseModel
{
    [Key]
    [Required]
    public string Id { get; set; }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(TypeName = "datetime")]
    public DateTime RecCreated { get; set; }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column(TypeName = "datetime")]
    public DateTime RecModified { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; }
}
