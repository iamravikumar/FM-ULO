using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using GSA.UnliquidatedObligations.BusinessLayer.Data;
using GSA.UnliquidatedObligations.Web.Tests.TestData;
using Moq;

namespace GSA.UnliquidatedObligations.Web.Tests.Mocks
{
    internal class DbContextMock
    {
        private readonly string PERSONUSERID;
        private readonly string GROUPUSERID;
        private readonly int ULOID;
        private readonly int WORKFLOWID;
        private readonly string WORKFLOWKEY;
        private readonly string CURRENTWORKFLOWACTIVITYKEY;
        private int REQUESTFORREASSIGNMENTID;
        private readonly int DOCUMENTID;
        private readonly int DOCUMENTTYPEID;
        private readonly List<AspNetUser> USERDATA;

        public DbContextMock(string personUserId = "", string groupUserId = "", int uloId = 1, int workflowId = 1, string workflowKey = "", string currentWorkFlowActivityKey = "", int requestForReassignmentId = 1, int documentId = 1, int documentTypeId = 1, List<AspNetUser> userdata = null)
        {
            PERSONUSERID = personUserId == "" ? Guid.NewGuid().ToString() : personUserId;
            GROUPUSERID = groupUserId == "" ? Guid.NewGuid().ToString() : groupUserId;
            ULOID =  uloId;
            WORKFLOWID = workflowId;
            WORKFLOWKEY = workflowKey == "" ? Guid.NewGuid().ToString() : workflowKey;
            CURRENTWORKFLOWACTIVITYKEY = currentWorkFlowActivityKey == "" ? "B2" : currentWorkFlowActivityKey;
            REQUESTFORREASSIGNMENTID = requestForReassignmentId;
            DOCUMENTID = documentId;
            DOCUMENTTYPEID = documentTypeId;
            USERDATA = GenerateUserData(userdata);
        }

        private List<AspNetUser> GenerateUserData(List<AspNetUser> userData = null)
        {
            if (userData != null)
                return userData;

            var personUserData = UsersData.GenerateData(5, PERSONUSERID);
            var groupUserData = UsersData.GenerateData(1, GROUPUSERID, "Group");
            return personUserData.Concat(groupUserData).ToList();
        }

        public ULODBEntities SetUpEntityMocks()
        {
            var genericUloList = UloData.GenerateData(10, ULOID, USERDATA);
            var regionUloList = UloData.GenerateRegionData(10, 4);
            var uloList = genericUloList.Concat(regionUloList).AsQueryable();
            var workflowList = WorkflowData.GenerateData(10, WORKFLOWID, USERDATA, PERSONUSERID, WORKFLOWKEY, CURRENTWORKFLOWACTIVITYKEY, regionUloList).AsQueryable();
            var userUsersList = UserUsersData.GenerateData(1, PERSONUSERID, GROUPUSERID).AsQueryable();
            var requestForReassignmentList = RequestForReassignmentData.GenerateData(10, REQUESTFORREASSIGNMENTID, WORKFLOWID, PERSONUSERID).AsQueryable();
            var userList = USERDATA.AsQueryable();
            var unliqudatedObjectsWorkflowQuestionsList = UnliqudatedObjectsWorkflowQuestionsData.GenerateData(20);
            var unliqudatedObjectsWorkflowQuestionsListQueryable = unliqudatedObjectsWorkflowQuestionsList.AsQueryable();
            var documentsList = DocumentData.GenerateData(10, PERSONUSERID, documentId: DOCUMENTID, documentTypeId: DOCUMENTTYPEID).AsQueryable();
            var documentTypesList = DocumentTypesData.GenerateData(5, "Contract", DOCUMENTTYPEID).AsQueryable();

            var claimsListPerson = AspNetUserClaimsData.GenerateData(10, PERSONUSERID);
            var claimsListGroup = AspNetUserClaimsData.GenerateData(5, GROUPUSERID);

            var claimsList = claimsListPerson.Concat(claimsListGroup).AsQueryable();
            
            //var notesList = NotesData.GenerateData(3, ULOID, userData).AsQueryable();

            var mockUloSet = new Mock<DbSet<UnliquidatedObligation>>();
            mockUloSet.As<IDbAsyncEnumerable<UnliquidatedObligation>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<UnliquidatedObligation>(uloList.GetEnumerator()));
            mockUloSet.As<IQueryable<UnliquidatedObligation>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UnliquidatedObligation>(uloList.Provider));
            mockUloSet.As<IQueryable<UnliquidatedObligation>>().Setup(m => m.Expression).Returns(uloList.Expression);
            mockUloSet.As<IQueryable<UnliquidatedObligation>>().Setup(m => m.ElementType).Returns(uloList.ElementType);
            mockUloSet.As<IQueryable<UnliquidatedObligation>>().Setup(m => m.GetEnumerator()).Returns(uloList.GetEnumerator());
            mockUloSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockUloSet.Object);

            var mockWorkflowSet = new Mock<DbSet<Workflow>>();
            mockWorkflowSet.As<IDbAsyncEnumerable<Workflow>>()
               .Setup(m => m.GetAsyncEnumerator())
               .Returns(new TestDbAsyncEnumerator<Workflow>(workflowList.GetEnumerator()));
            mockWorkflowSet.As<IQueryable<Workflow>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<Workflow>(workflowList.Provider));
            mockWorkflowSet.As<IQueryable<Workflow>>().Setup(m => m.Expression).Returns(workflowList.Expression);
            mockWorkflowSet.As<IQueryable<Workflow>>().Setup(m => m.ElementType).Returns(workflowList.ElementType);
            mockWorkflowSet.As<IQueryable<Workflow>>().Setup(m => m.GetEnumerator()).Returns(workflowList.GetEnumerator());
            mockWorkflowSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockWorkflowSet.Object);

            var mockUserUsersSet = new Mock<DbSet<UserUser>>();
            mockUserUsersSet.As<IDbAsyncEnumerable<UserUser>>()
               .Setup(m => m.GetAsyncEnumerator())
               .Returns(new TestDbAsyncEnumerator<UserUser>(userUsersList.GetEnumerator()));
            mockUserUsersSet.As<IQueryable<Workflow>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UserUser>(userUsersList.Provider));
            mockUserUsersSet.As<IQueryable<UserUser>>().Setup(m => m.Expression).Returns(userUsersList.Expression);
            mockUserUsersSet.As<IQueryable<UserUser>>().Setup(m => m.ElementType).Returns(userUsersList.ElementType);
            mockUserUsersSet.As<IQueryable<UserUser>>().Setup(m => m.GetEnumerator()).Returns(userUsersList.GetEnumerator());
            mockUserUsersSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockUserUsersSet.Object);

            var mockRequestForReassignmentSet = new Mock<DbSet<RequestForReassignment>>();
            mockRequestForReassignmentSet.As<IDbAsyncEnumerable<RequestForReassignment>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<RequestForReassignment>(requestForReassignmentList.GetEnumerator()));
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<RequestForReassignment>(requestForReassignmentList.Provider));
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>().Setup(m => m.Expression).Returns(requestForReassignmentList.Expression);
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>().Setup(m => m.ElementType).Returns(requestForReassignmentList.ElementType);
            mockRequestForReassignmentSet.As<IQueryable<RequestForReassignment>>().Setup(m => m.GetEnumerator()).Returns(requestForReassignmentList.GetEnumerator());

            var mockUserSet = new Mock<DbSet<AspNetUser>>();
            mockUserSet.As<IDbAsyncEnumerable<AspNetUser>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<AspNetUser>(userList.GetEnumerator()));
            mockUserSet.As<IQueryable<AspNetUser>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<AspNetUser>(userList.Provider));
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.Expression).Returns(userList.Expression);
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.ElementType).Returns(userList.ElementType);
            mockUserSet.As<IQueryable<AspNetUser>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

            var mockQuestionsSet = new Mock<DbSet<UnliqudatedObjectsWorkflowQuestion>>();
            mockQuestionsSet.As<IDbAsyncEnumerable<UnliqudatedObjectsWorkflowQuestion>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<UnliqudatedObjectsWorkflowQuestion>(unliqudatedObjectsWorkflowQuestionsListQueryable.GetEnumerator()));
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<UnliqudatedObjectsWorkflowQuestion>(unliqudatedObjectsWorkflowQuestionsListQueryable.Provider));
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.Expression).Returns(unliqudatedObjectsWorkflowQuestionsListQueryable.Expression);
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.ElementType).Returns(unliqudatedObjectsWorkflowQuestionsListQueryable.ElementType);
            mockQuestionsSet.As<IQueryable<UnliqudatedObjectsWorkflowQuestion>>().Setup(m => m.GetEnumerator()).Returns(unliqudatedObjectsWorkflowQuestionsListQueryable.GetEnumerator());
            mockQuestionsSet.Setup(d => d.Add(It.IsAny<UnliqudatedObjectsWorkflowQuestion>())).Callback<UnliqudatedObjectsWorkflowQuestion>((s) => unliqudatedObjectsWorkflowQuestionsList.Add(s));

            var mockDocumentSet = new Mock<DbSet<Document>>();
            mockDocumentSet.As<IDbAsyncEnumerable<Document>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Document>(documentsList.GetEnumerator()));
            mockDocumentSet.As<IQueryable<Document>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<Document>(documentsList.Provider));
            mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Expression).Returns(documentsList.Expression);
            mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.ElementType).Returns(documentsList.ElementType);
            mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.GetEnumerator()).Returns(documentsList.GetEnumerator());
            //mockRequestForReassignmentSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockRequestForReassignmentSet.Object);

            var mockDocumentTypeSet = new Mock<DbSet<DocumentType>>();
            mockDocumentTypeSet.As<IDbAsyncEnumerable<DocumentType>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<DocumentType>(documentTypesList.GetEnumerator()));
            mockDocumentTypeSet.As<IQueryable<DocumentType>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<DocumentType>(documentTypesList.Provider));
            mockDocumentTypeSet.As<IQueryable<DocumentType>>().Setup(m => m.Expression).Returns(documentTypesList.Expression);
            mockDocumentTypeSet.As<IQueryable<DocumentType>>().Setup(m => m.ElementType).Returns(documentTypesList.ElementType);
            mockDocumentTypeSet.As<IQueryable<DocumentType>>().Setup(m => m.GetEnumerator()).Returns(documentTypesList.GetEnumerator());

            var mockClaimsSet = new Mock<DbSet<AspNetUserClaim>>();
            mockClaimsSet.As<IDbAsyncEnumerable<AspNetUserClaim>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<AspNetUserClaim>(claimsList.GetEnumerator()));
            mockClaimsSet.As<IQueryable<AspNetUserClaim>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<AspNetUserClaim>(claimsList.Provider));
            mockClaimsSet.As<IQueryable<AspNetUserClaim>>().Setup(m => m.Expression).Returns(claimsList.Expression);
            mockClaimsSet.As<IQueryable<AspNetUserClaim>>().Setup(m => m.ElementType).Returns(claimsList.ElementType);
            mockClaimsSet.As<IQueryable<AspNetUserClaim>>().Setup(m => m.GetEnumerator()).Returns(claimsList.GetEnumerator());

            var mockULODBEntities = new Mock<ULODBEntities>();
            mockULODBEntities.Setup(c => c.UnliquidatedObligations).Returns(mockUloSet.Object);
            mockULODBEntities.Setup(c => c.Workflows).Returns(mockWorkflowSet.Object);
            mockULODBEntities.Setup(c => c.UserUsers).Returns(mockUserUsersSet.Object);
            mockULODBEntities.Setup(c => c.RequestForReassignments).Returns(mockRequestForReassignmentSet.Object);
            mockULODBEntities.Setup(c => c.AspNetUsers).Returns(mockUserSet.Object);
            mockULODBEntities.Setup(c => c.UnliqudatedObjectsWorkflowQuestions).Returns(mockQuestionsSet.Object);
            mockULODBEntities.Setup(c => c.Documents).Returns(mockDocumentSet.Object);
            mockULODBEntities.Setup(c => c.DocumentTypes).Returns(mockDocumentTypeSet.Object);
            mockULODBEntities.Setup(c => c.AspNetUserClaims).Returns(mockClaimsSet.Object);
            return mockULODBEntities.Object;

        }

      

    }
}
