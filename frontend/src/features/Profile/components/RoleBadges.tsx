import type { MarketRole } from '../types';
import './RoleBadges.css';

export function RoleBadges({ roles }: Readonly<{ roles: readonly MarketRole[] }>) {
  if (roles.length === 0) {
    return null;
  }

  return (
    <ul className="role-badges" aria-label="Roles">
      {roles.map((role) => (
        <li key={role} className={`role-badge role-badge-${role.toLowerCase()}`}>
          {role}
        </li>
      ))}
    </ul>
  );
}
