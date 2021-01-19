using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DS.Identity.Multitenancy
{
    public class Tenant
    {
        public Tenant()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual string Id { get; set; }
        
        public virtual string Name { get; set; }
        
        public virtual WebAuthnCredential AdminKey { get; set; }
        
        public virtual IEnumerable<WebAuthnCredential> DeviceKeys { get; set; }
    }
}