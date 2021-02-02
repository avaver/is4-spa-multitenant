using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DS.Identity.Multitenancy
{
    public class Tenant
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string AdminKeyId { get; set; }
        
        public ICollection<WebAuthnCredential> Keys { get; set; }
    }
}