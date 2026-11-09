using System;
using System.ComponentModel.DataAnnotations;

namespace SdnnGa.Model.Database.Models;

public class BaseModel
{
    [Key]
    [Required]
    public string Id { get; set; }

    [Required]
    public DateTime RecCreated { get; set; }

    [Required]
    public DateTime RecModified { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; }
}