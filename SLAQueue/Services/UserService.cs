using SLAQueue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SLAQueue.Services
{
    /// <summary>
    /// This class manages users that send documents. For the sake of brevity, users cannot be deleted and all information is stored in a static context.
    /// </summary>
    public class UserService
    {
        /// <summary>
        /// Stores users to be accessed by the service
        /// </summary>
        private static Dictionary<Guid, User> Users = new Dictionary<Guid, User>();

        /// <summary>
        /// Creates a new user with the specified SLA class
        /// </summary>
        /// <param name="slaClass">SLA class</param>
        /// <returns>ID of the new user</returns>
        public static Guid CreateUser(SLAClass slaClass)
        {
            var newId = Guid.NewGuid();

            Users.Add(newId, new User { Id = newId, SLAClass = slaClass });
            Console.WriteLine("Added user with SLA class " + slaClass + " and ID " + newId);
            return newId;
        }

        /// <summary>
        /// Returns a user with the requested ID, or null if none found
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>The user, or null if none found</returns>
        public static User GetUser(Guid userId)
        {
            if( Users.TryGetValue(userId, out User user))
            {
                return user;
            }

            Console.WriteLine("Error: no user with ID" + userId);
            return null;
        }

        public static IEnumerable<User> GetUsers()
        {
            return Users.Select(x => x.Value);
        }
      
    }
}
