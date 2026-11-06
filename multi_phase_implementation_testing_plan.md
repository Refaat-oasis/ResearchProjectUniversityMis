# خطة التنفيذ المرحلية للمنصة (Multi-Phase Implementation & Testing Plan)

تعتمد الخطة على **ASP.NET Core MVC** و **SQL Server** و **ASP.NET Core Identity**، وتغطي بوابات المنصة الثلاث (الباحثين، المحكمين وأعضاء اللجنة، لوحة التحكم الإدارية) ودورة العمل ذات الخطوات الست. وتلتزم بالقواعد التالية (التفاصيل في `AGENTS.md`):

* **جدول مستخدمين واحد (`AspNetUsers` / `ApplicationUser`)** والأدوار: `SuperAdmin`, `CollegeCoAdmin`, `Judge`, `Researcher`. جداول `Researcher` و `CollegeCoAdmin` و `Judge` ملفات تعريفية مرتبطة بـ `UserId`. `SuperAdmin` دور فقط بلا جدول.
* **تسجيل الباحث ببريد مؤسسي مع تأكيد البريد** (متطلب SSO في المستند)، والمحكمون بدعوة بالبريد (قد يكونون خبراء خارجيين).
* **يمكن للباحث أن يكون محكماً** بنفس الحساب، مع **منع تحكيم بحث هو أحد باحثيه**، وحصر ما يراه المحكم في الأبحاث المسندة إليه.
* **أكثر من محكم للبحث الواحد** بتقييم مستقل وتوقيع رقمي وعدة جولات تقييم، **بدون تصويت**: يُعتمد البحث إذا وافق جميع المحكمين، وإذا طلب أحدهم تعديلات يحتاج البحث مزيداً من العمل.
* **الأدوار أربعة فقط:** مدير النظام، مدير اللجنة الفرعية لكل كلية، المحكم، الباحث. **ولا يتدخل مدير النظام في الاعتماد النهائي وإصدار الشهادة** (للعرض فقط).
* **Controller و Repository مستقل لكل طرف** بدون Generic Repository، وواجهة عربية RTL بقائمة جانبية يمنى وجميع الشاشات Partial Views.
* **الحالات الخمس:** `قيد المراجعة الشكلية` ← `مسند للمحكمين` ← `تعديلات مطلوبة` ← `معتمد` ← `صادر الشهادة`.

---

## الهيكل العام للمشروع (Solution Folder Structure)

```text
ResearchEthicsPlatform/
├── Controllers/
│   ├── AccountController.cs             # الدخول الموحد + تسجيل الباحثين + تأكيد البريد + قبول دعوة التحكيم
│   ├── SuperAdminController.cs          # كل شيء + الكليات والتصنيفات وحقولها + الـ Co-Admins والمحكمين + التقارير والأرشيف
│   ├── CollegeCoAdminController.cs      # فحص أبحاث الكلية + دعوة واقتراح وإسناد المحكمين + الاعتماد وإصدار الشهادة
│   ├── JudgeController.cs               # تقييم الأبحاث المسندة فقط + التوقيع
│   ├── ResearcherController.cs          # التقديم + المرفقات والتعديلات + تتبع الحالات + الشهادات
│   └── PublicController.cs              # صفحة التحقق العامة من الشهادة (بدون دخول)
├── Data/
│   ├── ApplicationDbContext.cs          # IdentityDbContext<ApplicationUser> + علاقات الجداول التسعة والمساندة
│   └── DbSeeder.cs                      # الأدوار الأربعة + حساب الـ Super Admin
├── Models/
│   ├── Constants/                       # ResearchStatus / Role / InvitationStatus / ResearcherType / AttachmentType / EvaluationDecision / EthicsRating
│   └── Entities/                        # ApplicationUser + الكلاسات التسعة + JudgeInvitation + ResearchStatusHistory
├── Services/
│   ├── IEmailSender.cs / EmailSender.cs
│   ├── ICertificateService.cs           # رقم الشهادة + QR + PDF + توقيع مدير اللجنة
│   └── IFileStorageService.cs           # حفظ المرفقات خارج wwwroot
├── Repositories/
│   ├── Interfaces/   (IAccount / ISuperAdmin / ICollegeCoAdmin / IJudge / IResearcher / IPublic)Repository.cs
│   └── Implementations/
└── Views/
    ├── Shared/        _Layout.cshtml, _RightSidebarPartial.cshtml
    ├── Account/       Login.cshtml, SignUp.cshtml, AcceptJudgeInvitation.cshtml
    ├── Public/        VerifyCertificate.cshtml
    ├── SuperAdmin/    _OverviewDashboardPartial, _ManageCollegesPartial, _ManageCategoriesPartial (تعريف الحقول),
    │                  _ManageCoAdminsPartial, _ManageJudgesPartial, _AllUniversityResearchesPartial,
    │                  _ReportsPartial, _CertificatesArchivePartial
    ├── CollegeCoAdmin/ _ScreeningQueuePartial, _InviteJudgePartial, _AssignJudgePartial,
    │                  _FinalApprovalPartial, _IssueCertificatePartial, _CollegeReportsPartial
    ├── Judge/         _AssignedResearchesPartial, _EvaluationFormPartial
    └── Researcher/    _CreateResearchPartial, _DynamicCategoryFieldsPartial, _MyResearchesPartial,
                       _UploadModificationsPartial, _ResearchStatusTrackerPartial, _MyCertificatesPartial
```

---

## المرحلة الأولى (Phase 1): Identity والكلاسات وقاعدة البيانات

### 1. مهام التنفيذ
1. **Identity:** `ApplicationUser : IdentityUser` بخاصية `FullName` فقط، و`ApplicationDbContext : IdentityDbContext<ApplicationUser>`.
2. **الكلاسات التسعة** (`College`, `CollegeCoAdmin`, `Researcher`, `ResearchCategory`, `Research`, `ResearcherResearchesBridge`, `ResearchAttachment`, `Judge`, `Evaluation`): خصائص ومفاتيح أجنبية وNavigation ورسائل Data Annotations عربية فقط، بدون منطق. بدون بريد أو كلمة مرور في الملفات التعريفية.
3. **الأعمدة المضافة** (راجع القسم 1.6 في `AGENTS.md`): تعريف حقول التصنيف (`FieldsDefinitionJson`) وإجابات الباحث (`DynamicFieldsJson`) و`AcademicSpecialty` و`SubmittedAt` و`ScreeningDueAt` وحقول الشهادة وحقول التقييم (المعايير الثلاثة والقرار والتوقيع) و`SignatureImagePath` و`Judge.Specialty/IsActive` و`ResearchAttachment.AttachmentType/Version`.
4. **الجدولان المساندان:** `JudgeInvitation` و `ResearchStatusHistory`.
5. **الثوابت:** الحالات الخمس + الأدوار + حالات الدعوة + فئات الباحثين + أنواع المرفقات + قرارات التقييم + تقديرات المعايير الأخلاقية.
6. **Fluent API:** فهارس `UserId` الفريدة على الملفات الثلاثة، وفهرس `Evaluation` الفريد على `(ResearchId, JudgeId, RoundNumber)`، و`DeleteBehavior.Restrict`.
7. **الإعدادات و`DbSeeder`:** الأدوار الأربعة وحساب الـ `SuperAdmin` (من User Secrets)، ومفاتيح `Auth:AllowedEmailDomains` و`Screening:SlaHours` و`Evaluation:MinJudgesPerResearch` و`Specialties`.

### 2. اختبارات المرحلة الأولى
* [ ] **Migration:** إنشاء جداول Identity والجداول التسعة والمساندين بكل المفاتيح الأجنبية.
* [ ] **فرادة الملف التعريفي:** صفان في `Judge` لنفس `UserId` يفشلان.
* [ ] **التقييم:** تقييمان من محكمين مختلفين لنفس البحث والجولة ينجحان، وتكرار (بحث، محكم، جولة) يفشل.
* [ ] **Seeding:** وجود الأدوار الأربعة وحساب الـ `SuperAdmin`.
* [ ] **رسائل التحقق العربية** تظهر عند البيانات الناقصة.

---

## المرحلة الثانية (Phase 2): الدخول الموحد والتسجيل بالبريد المؤسسي والواجهة العربية

### 1. مهام التنفيذ
1. **`AccountRepository` و `AccountController`:**
   * **`Login.cshtml`** واحدة لجميع الأدوار عبر `SignInManager` مع Lockout. عند تعدد الأدوار تُفتح لوحة الدور الأعلى أولوية.
   * **`SignUp.cshtml`** للباحث (الكلية، الاسم، البريد، كلمة المرور، فئة الباحث، الدرجة العلمية، القسم، الرقم القومي، الهاتف):
     * رفض أي بريد خارج `Auth:AllowedEmailDomains` برسالة عربية.
     * **تأكيد البريد إجباري** قبل أول دخول (رابط يرسل عبر `IEmailSender`).
     * إنشاء المستخدم + دور `Researcher` + صف `Researcher` في Transaction واحدة.
2. **`IEmailSender`** برسائل عربية (تأكيد البريد، الدعوات، كلمات المرور المؤقتة، الإشعارات).
3. **`_Layout.cshtml` و `_RightSidebarPartial.cshtml`:** RTL كامل؛ `SuperAdmin` يرى كل الأقسام، وغيره يرى أقسام **كل أدواره** (قسم الباحث، قسم المحكم، قسم الإدارة).
4. **دالة AJAX موحدة** لتحميل الـ Partials داخل `#main-content-container` مع Anti-Forgery Token ودعم الفتح المباشر عبر `#hash`.

### 2. اختبارات المرحلة الثانية
* [ ] **تسجيل باحث ببريد مؤسسي:** نجاح التسجيل، منع الدخول قبل تأكيد البريد، ثم الدخول بعد التأكيد.
* [ ] **رفض بريد غير مؤسسي** عند تسجيل الباحث.
* [ ] **الدخول الموحد للأدوار الأربعة** وتغير القائمة الجانبية حسب الصلاحية.
* [ ] **تعدد الأدوار:** حساب بدوري `Researcher` و `Judge` يرى القسمين.
* [ ] **Lockout** بعد محاولات كلمة مرور خاطئة متكررة.

---

## المرحلة الثالثة (Phase 3): موديول مدير النظام العام (الكليات، التصنيفات وحقولها، الـ Co-Admins، دعوات المحكمين)

### 1. مهام التنفيذ
1. **`SuperAdminRepository` و `SuperAdminController`:**
   * الكليات والتصنيفات البحثية (بشري، حيواني، تجارب سريرية، استبيانات اجتماعية...) مع **شاشة تعريف الحقول الإضافية لكل تصنيف** (`FieldsDefinitionJson`: نوع الحقل، التسمية العربية، إلزامي أم لا، خيارات القوائم).
   * **مديرو اللجان الفرعية:** بريد جديد ← مستخدم بكلمة مرور مؤقتة (`EmailConfirmed = true`، تغيير إجباري عند أول دخول)؛ مستخدم موجود ← إضافة دور `CollegeCoAdmin` لنفس الحساب؛ ثم إنشاء صف `CollegeCoAdmin` وربطه بالكلية، مع رفع **صورة توقيعه** (تُختم على الشهادات التي يصدرها).
   * **دعوة المحكمين وإدارتهم** (`InviteJudgeAsync`, `GetAllJudgesAsync`, `SetJudgeActiveAsync`, `RevokeInvitationAsync`).
   * **الرؤية الشاملة** لكل الأبحاث والتقييمات والمرفقات وسجل الحالات والشهادات.
2. **آلية دعوة المحكم** (مشتركة مع الـ Co-Admin):
   * Token عشوائي (32 بايت) ويُخزن **Hash (SHA-256)** فقط، صلاحية 7 أيام.
   * **مستخدم موجود (مثل باحث):** صفحة تأكيد تتطلب تسجيل الدخول وتطابق البريد، فيضاف دور `Judge` وصف `Judge` لنفس الحساب.
   * **مستخدم جديد:** `AcceptJudgeInvitation.cshtml` لإدخال الاسم وكلمة المرور (دون اشتراط بريد مؤسسي، `EmailConfirmed = true`).
   * رفض الرابط المستخدم أو المنتهي أو الملغى. القبول داخل `AccountRepository`.
3. **الـ Partials:** `_OverviewDashboardPartial`, `_ManageCollegesPartial`, `_ManageCategoriesPartial`, `_ManageCoAdminsPartial`, `_ManageJudgesPartial`, `_AllUniversityResearchesPartial`.

### 2. اختبارات المرحلة الثالثة
* [ ] **إضافة `CollegeCoAdmin`:** كلية جديدة ← Co-Admin ← دخوله من الصفحة الموحدة وتغيير كلمة المرور المؤقتة.
* [ ] **Co-Admin من باحث موجود:** نفس الحساب يحصل على الدور الإضافي بلا حساب ثانٍ.
* [ ] **تعريف حقول تصنيف** (الآلية فقط؛ الحقول الفعلية لم تُحدد بعد ولا تُختلق) وظهورها لاحقاً في نموذج الباحث.
* [ ] **دعوة محكم جديد** ثم الدخول بدور `Judge`، **ودعوة باحث موجود** وقبولها بنفس الحساب مع ظهور القسمين في القائمة.
* [ ] **أمان الدعوة:** رفض المنتهي/المستخدم/الملغى، ورفض القبول عند اختلاف بريد المستخدم، وعدم تخزين الـ Token الخام.
* [ ] **تعطيل المحكم:** يختفي من قوائم الإسناد وتبقى تقييماته القديمة.
* [ ] **صلاحية الـ Super Admin الشاملة** على كل الـ Partials (عرضاً)، مع استثناء الاعتماد النهائي وإصدار الشهادة.

---

## المرحلة الرابعة (Phase 4): بوابة الباحث وتقديم البحث
**الحالة المستهدفة:** `قيد المراجعة الشكلية`

### 1. مهام التنفيذ
1. **`ResearcherRepository`:**
   * `GetCategoriesByCollegeIdAsync` و`GetCategoryFieldsAsync(categoryId)`.
   * `CreateResearchWithAttachmentsAsync(...)`: يتحقق من إجابات الحقول الديناميكية على الخادم مقابل تعريف التصنيف، ويحفظها في `DynamicFieldsJson`، ويضبط `AcademicSpecialty` و`SubmittedAt` و`ScreeningDueAt` (= التقديم + `Screening:SlaHours`)، ويضبط الحالة `قيد المراجعة الشكلية`، ويربط الباحث بـ `ResearcherResearchesBridge`، ويحفظ المرفقات الإلزامية (**الموافقة المستنيرة، أدوات البحث، خطة البحث**) عبر `IFileStorageService` (خارج `wwwroot`، مع التحقق من النوع والحجم)، ويسجل `ResearchStatusHistory`، ويرسل إيصال التقديم للباحث وإشعاراً للـ Co-Admin.
   * `GetResearcherProposalsAsync`: أبحاث الباحث (مؤلفاً) فقط.
   * `SubmitModifiedAttachmentsAsync`: مسموح فقط عند الحالة `تعديلات مطلوبة` ومن أحد باحثي البحث؛ يحفظ المرفقات بنسخة (`Version`) جديدة، وتعود الحالة إلى `مسند للمحكمين` وتبدأ جولة جديدة (يكتمل منطقها في المرحلة 6).
2. **الـ Partials:** `_CreateResearchPartial`, `_DynamicCategoryFieldsPartial` (يرسم الحقول من تعريف التصنيف), `_MyResearchesPartial`, `_ResearchStatusTrackerPartial` (شريط الحالات الخمس مع تحديث دوري كل 30 ثانية), `_UploadModificationsPartial`.

### 2. اختبارات المرحلة الرابعة
* [ ] **إنشاء بحث** بحالة `قيد المراجعة الشكلية` مع مرفقاته الثلاثة وسجل الحالة وحقوله الديناميكية.
* [ ] **تغير الحقول بتغير نوع البحث** (بشري/حيواني/سريري/استبيان)، ورفض الحقول الإلزامية الناقصة.
* [ ] **ظهور البحث** لـ Co-Admin نفس الكلية وللـ `SuperAdmin` فقط، وليس لـ Co-Admin كلية أخرى.
* [ ] **الملكية:** باحث لا يفتح أو يحمّل مرفقات بحث ليس من مؤلفيه (حتى بتخمين الرابط).
* [ ] **رفض المرفقات** ذات النوع أو الحجم غير المسموح.
* [ ] **التتبع:** شريط الحالة يعكس الحالة الحالية، ووصول بريد الإيصال.

---

## المرحلة الخامسة (Phase 5): موديول مدير اللجنة الفرعية (الفحص الشكلي، التوزيع، الإسناد)
**الحالة المستهدفة:** `مسند للمحكمين`

### 1. مهام التنفيذ
1. **`CollegeCoAdminRepository`** (كل الاستعلامات مفلترة بكلية الـ Co-Admin؛ الـ `SuperAdmin` فقط يتجاوز):
   * `GetCollegeResearchesByStatusAsync`: مع عرض المهلة `ScreeningDueAt` وتمييز **المتأخر عن 24–48 ساعة**.
   * `GetResearchWithAttachmentsAsync` و`SaveScreeningNotesAsync`.
   * **`InviteJudgeAsync`:** دعوة محكم بالبريد أو باختيار باحث موجود (نفس آلية المرحلة 3).
   * **`SuggestJudgesAsync(researchId)` (التوزيع الآلي):** المحكمون النشطون مرتبين حسب تطابق `Judge.Specialty` مع `Research.AcademicSpecialty` ثم قلة التقييمات المفتوحة، **مع استبعاد باحثي البحث**.
   * **`AssignJudgesToResearchAsync(researchId, judgeIds)`:** يتحقق من عدم تضارب المصالح (ويرفض بخطأ عربي)، ومن عدد المحكمين ≥ `Evaluation:MinJudgesPerResearch`، وينشئ صف `Evaluation` لكل محكم برقم الجولة الحالي بقرار `قيد التقييم`، ويحول الحالة إلى `مسند للمحكمين`، ويسجل `ResearchStatusHistory`، ويرسل إشعاراً للمحكمين.
2. **الـ Partials:** `_ScreeningQueuePartial`, `_InviteJudgePartial`, `_AssignJudgePartial` (مراجعة المرفقات + قائمة المحكمين المقترحين مرتبة + اختيار واحد أو أكثر).

### 2. اختبارات المرحلة الخامسة
* [ ] **إسناد محكم** ← تغير الحالة وإنشاء `Evaluation` بجولة 1 ووصول الإشعار للمحكم.
* [ ] **إسناد أكثر من محكم** لنفس البحث والجولة.
* [ ] **ترتيب الاقتراحات:** المحكم المطابق للتخصص يظهر أولاً، والأقل عبئاً عند التساوي.
* [ ] **تضارب المصالح:** باحث-محكم لا يظهر في اقتراحات بحثه ويُرفض الإسناد المباشر (بتلاعب بالطلب)، لكن يمكن إسناد بحث آخر إليه.
* [ ] **عزل الكليات:** Co-Admin كلية (أ) لا يرى ولا يسند بحث كلية (ب).
* [ ] **مهلة الفحص:** يظهر التنبيه للبحث المتأخر عن `ScreeningDueAt`.
* [ ] **الانتقالات:** رفض أي انتقال غير مسموح.

---

## المرحلة السادسة (Phase 6): موديول المحكم العلمي (التقييم، التعديلات، الاعتماد)
**الحالات المستهدفة:** `تعديلات مطلوبة` ↔ `مسند للمحكمين` ← `معتمد`

### 1. مهام التنفيذ
1. **`JudgeRepository`:**
   * `GetAssignedResearchesForJudgeAsync`: **فقط** الأبحاث التي لها `Evaluation` بنفس `JudgeId`، مع استبعاد أي بحث يكون المحكم أحد باحثيه. الدور `Judge` لا يمنح رؤية كل الأبحاث.
   * `SubmitEvaluationAsync(...)`: يسجل معايير التقييم الأخلاقي (**حماية الخصوصية، نسبة المخاطر، الموافقة المستنيرة**) والتوصيات والقرار (`موافق` / `طلب تعديلات` مع ملاحظات إلزامية) ويفعّل `IsDigitallySigned` و`SignedAt`.
   * `TryCloseRoundAsync` داخل نفس الـ Transaction: عند اكتمال تقييمات الجولة، إن وافق الجميع ← **`معتمد`**، وإلا ← **`تعديلات مطلوبة`** مع جمع الملاحظات (دون كشف هوية المحكم للباحث). كل انتقال يسجل في `ResearchStatusHistory` ويرسل بريداً.
   * كل دالة تعيد التحقق من ملكية الـ `Evaluation` للمحكم الحالي، ومن أنه ليس أحد باحثي البحث، ومن أن الجولة هي الحالية، ومن عدم تعديل تقييم سبق تقديمه.
   * لا يرى المحكم تقييمات الآخرين أثناء الجولة المفتوحة.
2. **استكمال إعادة الرفع لدى الباحث:** عند رفع التعديلات تُنشأ صفوف جولة جديدة للمحكمين السابقين (ويمكن للـ Co-Admin إعادة الإسناد إن أصبح أحدهم غير نشط) وتعود الحالة إلى `مسند للمحكمين`.
3. **الـ Partials:** `_AssignedResearchesPartial`, `_EvaluationFormPartial`.

### 2. اختبارات المرحلة السادسة
* [ ] **نطاق المحكم:** يرى بحثه المسند فقط ولا يفتح غيره حتى بتغيير المعرف في الرابط.
* [ ] **الباحث-المحكم:** يرى أبحاثه في قسم الباحث وأبحاث التحكيم المسندة في قسم المحكم، ولا يستطيع فتح تقييم بحثه الخاص.
* [ ] **قاعدة الحسم بمحكمين اثنين (بدون تصويت):** موافقة الاثنين ← `معتمد`؛ موافقة واحد وطلب تعديلات من الآخر ← `تعديلات مطلوبة`؛ الحالة لا تتغير قبل اكتمال الجولة.
* [ ] **الاستقلال:** المحكم لا يرى قرار زميله أثناء الجولة، والباحث يرى الملاحظات دون أسماء.
* [ ] **مسار التعديلات:** طلب تعديل ← ظهور الحالة لدى الباحث ← رفع الملف المعدل ← عودة `مسند للمحكمين` وجولة 2.
* [ ] **منع التعديل بعد التقديم** ومنع التقديم على جولة غير حالية.

---

## المرحلة السابعة (Phase 7): الاعتماد النهائي وإصدار الشهادة والتحقق
**الحالة المستهدفة:** `صادر الشهادة`

### 1. مهام التنفيذ
1. **`IssueEthicalCertificateAsync`** (في `CollegeCoAdminRepository` **فقط**، ولا يوجد إصدار للـ `SuperAdmin`): للأبحاث `معتمد` فقط؛ يعرض مدير اللجنة التقرير النهائي (تقييمات المحكمين) في `_FinalApprovalPartial` ثم **"اعتماد وإصدار"** فتولّد `ICertificateService` رقماً فريداً مرئياً ورابط تحقق و**QR** و**PDF** مختوماً بـ**توقيع مدير اللجنة المُصدِر** (ويُمنع الإصدار إن لم يرفع توقيعه)، ويحفظ `CertificateFilePath` و`CertificateIssuedAt`، ويحول الحالة إلى `صادر الشهادة`.
2. **الإرسال والحفظ:** إرسال الـ PDF بالبريد لكل باحثي البحث، وإتاحته من `_MyCertificatesPartial`. فشل البريد يُسجل ولا يلغي الإصدار.
3. **`PublicController` و `IPublicRepository` و `VerifyCertificate.cshtml`:** صفحة عامة يفتحها الـ QR تعرض صحة الرقم والحد الأدنى من البيانات فقط.
4. **`_IssueCertificatePartial`:** عرض الشهادة النهائية للطباعة والتحميل.

### 2. اختبارات المرحلة السابعة
* [ ] **الدورة الكاملة:** `قيد المراجعة الشكلية` ← `مسند للمحكمين` ← `تعديلات مطلوبة` ← `مسند للمحكمين` ← `معتمد` ← `صادر الشهادة`، مع مراقبتها من الحسابات الأربعة واكتمال `ResearchStatusHistory`.
* [ ] **الشهادة:** رقم فريد، وتوقيع مدير اللجنة، ووصول الـ PDF للباحثين، وظهورها في حساب الباحث.
* [ ] **التحقق العام:** مسح الـ QR يعرض البيانات الصحيحة، ورقم غير صالح لا يعرض شيئاً ولا يكشف مرفقات أو تقييمات.
* [ ] **الـ SuperAdmin لا يملك الاعتماد النهائي ولا الإصدار:** الأزرار غير ظاهرة له، وأي طلب مباشر للـ Action يُرفض.
* [ ] **الإصدار دون توقيع مرفوع** يعطي رسالة عربية واضحة.
* [ ] **منع إصدار** شهادة لبحث لم يصل إلى `معتمد`، ومنع Co-Admin كلية أخرى.
* [ ] **السيناريو الكامل لباحث-محكم:** نفس الحساب يقدم بحثاً ويحكّم بحثاً آخر دون تعارض.

---

## المرحلة الثامنة (Phase 8): التقارير والأرشيف والتحسينات والنشر

### 1. مهام التنفيذ
1. **التقارير والإحصائيات** (`_ReportsPartial` للـ Super Admin على كل الكليات، و`_CollegeReportsPartial` للـ Co-Admin على كليته): أعداد الأبحاث حسب الحالة والكلية والتصنيف، متوسط زمن الفحص والتحكيم، **نسبة الفحص ضمن `Screening:SlaHours`**، الشهادات الصادرة لكل فترة، مع **التصدير إلى Excel و PDF** (للاعتماد المؤسسي والجودة).
2. **الأرشيف الرقمي** (`_CertificatesArchivePartial`): بحث بالرقم والباحث والكلية والتاريخ.
3. **اختياري:** تسجيل دخول خارجي (SSO) عبر OpenID Connect مع حسابات الجامعة بنفس `ApplicationUser`، وتحديث لحظي للحالة عبر SignalR.
4. **النشر السحابي:** HTTPS إجباري، إعدادات سرية خارج الكود، نسخ احتياطي دوري لقاعدة البيانات والمرفقات، ومراقبة الأخطاء.

### 2. اختبارات المرحلة الثامنة
* [ ] **صحة الأرقام:** مطابقة أعداد التقارير مع بيانات اختبارية معروفة، وعدم رؤية Co-Admin لأرقام كلية أخرى.
* [ ] **التصدير:** ملفا Excel و PDF يفتحان بالمحتوى الصحيح وبالعربية سليمة.
* [ ] **الأرشيف:** العثور على شهادة بكل معيار بحث.
* [ ] **استعادة النسخة الاحتياطية** تجريبياً على بيئة اختبار.
* [ ] **اختبار أمني نهائي:** محاولة الوصول لمرفقات وأبحاث وتقييمات خارج النطاق عبر تغيير المعرفات، والتأكد من الرفض في كل مرة.
