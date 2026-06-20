import os
import re

repos = []
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('Repository.cs'):
            repos.append(os.path.join(root, f))

for repo in repos:
    with open(repo, 'r', encoding='utf-8') as f:
        content = f.read()
    
    if 'ApiDbContext' not in content:
        continue
        
    print(f"Modifying {repo}")
    
    # Imports
    content = content.replace('using Microsoft.EntityFrameworkCore;', 'using MongoDB.Driver;\nusing MongoDB.Driver.Linq;')
    
    # Class signature
    content = re.sub(r'\(ApiDbContext \w+\)', '(IMongoDbContext context)', content)
    content = re.sub(r'public \w+Repository\(ApiDbContext \w+\)', 'public \\g<0>Repository(IMongoDbContext context)', content)
    
    # _context type
    content = content.replace('private readonly ApiDbContext _context;', 'private readonly IMongoDbContext _context;')
    
    with open(repo, 'w', encoding='utf-8') as f:
        f.write(content)

print("Done")
