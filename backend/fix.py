import os
import re

base_dir = r'C:\Users\digob\Desktop\greg-company-ecosystem\backend'

def fix_about():
    p = os.path.join(base_dir, 'Features', 'About', 'Application', 'Services', 'AboutService.cs')
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # int to string in method signatures
    content = re.sub(r'public async Task UpdateSectionAsync\(int sectionId', r'public async Task UpdateSectionAsync(string sectionId', content)
    content = re.sub(r'public async Task DeleteSectionAsync\(int sectionId\)', r'public async Task DeleteSectionAsync(string sectionId)', content)
    content = re.sub(r'public async Task UpdateMemberAsync\(int memberId', r'public async Task UpdateMemberAsync(string memberId', content)
    content = re.sub(r'public async Task DeleteMemberAsync\(int memberId\)', r'public async Task DeleteMemberAsync(string memberId)', content)
    
    # fileId logic
    content = re.sub(r'int\? fileId = null;', r'string? fileId = null;', content)
    content = re.sub(r'var fileId = uploadedFile.Id;', r'var fileId = uploadedFile.Id.ToString();', content)
    content = re.sub(r'fileId = uploadedFile.Id;', r'fileId = uploadedFile.Id.ToString();', content)
    
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_home():
    p = os.path.join(base_dir, 'Features', 'Home', 'Application', 'Services', 'HomeService.cs')
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
        
    content = re.sub(r'hero\.FileId\.HasValue', r'(!string.IsNullOrEmpty(hero.FileId))', content)
    content = re.sub(r'hero\.FileId\.Value', r'hero.FileId', content)
    
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_support():
    p = os.path.join(base_dir, 'Features', 'Support', 'Application', 'Services', 'SupportService.cs')
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
        
    content = re.sub(r'await _repository\.UpdateAsync\(ticketId, status\);', r'ticket.Status = status;\n            await _repository.UpdateAsync(ticketId, ticket);', content)
    
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_videos():
    p = os.path.join(base_dir, 'Features', 'Videos', 'Application', 'Services', 'AdminVideoService.cs')
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
        
    content = re.sub(r'CourseId = int\.Parse\(videoDto\.CourseId\)', r'CourseId = videoDto.CourseId', content)
    content = re.sub(r'int\.Parse\(videoDto\.CourseId\)', r'videoDto.CourseId', content)
    content = re.sub(r'video\.FileId > 0', r'(!string.IsNullOrEmpty(video.FileId))', content)
    
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_admin_claim():
    p = os.path.join(base_dir, 'Features', 'MercadoPago', 'Claims', 'Application', 'Services', 'AdminClaimService.cs')
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
        
    content = re.sub(r'public async Task<ClaimDto\?> GetByMpClaimIdAsync\(long mpClaimId\)', r'public async Task<ClaimDto?> GetByMpClaimIdAsync(string mpClaimId)', content)
    content = re.sub(r'GetByMpClaimIdAsync\(long\.Parse\(mpClaimId\.ToString\(\)\)\)', r'GetByMpClaimIdAsync(mpClaimId.ToString())', content)
    content = re.sub(r'int\.Parse\((claim\.Id)\)', r'\1', content)
    
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_user_claim():
    p = os.path.join(base_dir, 'Features', 'MercadoPago', 'Claims', 'Application', 'Services', 'UserClaimService.cs')
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
        
    content = re.sub(r'GetByIdAsync\(int id\)', r'GetByIdAsync(string id)', content)
    content = re.sub(r'int\.Parse\((claim\.Id)\)', r'\1', content)
    
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_identity():
    p = os.path.join(base_dir, 'Extensions', 'Services', 'Persistence', 'IdentityServicesExtensions.cs')
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()
        
    # builder.Services.AddIdentityMongoDbProvider<Users, Roles, Guid>(mongoOptions => { mongoOptions.ConnectionString = mongoUrl; });
    content = re.sub(
        r'builder\.Services\.AddIdentityMongoDbProvider<Users, Roles, Guid>\(\s*mongoOptions =>\s*\{\s*mongoOptions\.ConnectionString = mongoUrl;\s*\}\s*\);',
        r'builder.Services.AddIdentityMongoDbProvider<Users, Roles, Guid>(identity => { }, mongo => { mongo.ConnectionString = mongoUrl; });',
        content
    )
    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)

try:
    fix_about()
    fix_home()
    fix_support()
    fix_videos()
    fix_admin_claim()
    fix_user_claim()
    fix_identity()
    print("Python fixes applied successfully")
except Exception as e:
    print(f"Error: {e}")
