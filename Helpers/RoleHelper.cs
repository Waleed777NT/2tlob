namespace _2tlob.Helpers
{
    public static class RoleHelper
    {
        private static readonly string[] PriorityOrder = { "Admin", "Seller", "Customer" };

        //
        // Returns the highest-priority role a user holds, for display purposes only.
        // Does NOT affect authorization - [Authorize(Roles=...)] and
        // User.IsInRole(...) checks are unaffected by this and keep working
        //off the full role set as before.
        //
        public static string GetDisplayRole(IEnumerable<string> roles)
        {
            var roleSet = roles as ICollection<string> ?? roles.ToList();

            foreach (var candidate in PriorityOrder)
            {
                if (roleSet.Contains(candidate))
                {
                    return candidate;
                }
            }

            return roleSet.FirstOrDefault() ?? "Customer";
        }
    }
}
