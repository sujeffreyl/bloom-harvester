using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloomHarvester.Parse.Model;
using NUnit.Framework;

namespace BloomHarvesterTests.Parse.Model
{
	class PointerTests
	{
		[Test]
		public void Pointer_GetJson_SimpleObject_CorrectJson()
		{
			// Note: We make a semi-interesting value to make sure it doesn't show up in the JSON,
			// but since it's a pointer, the value of the underlying should obviously not make a difference.
			var lang = new Language()
			{
				IsoCode = "en",
				EthnologueCode = "en",
				Name = "English"
			};

			var pointer = new Pointer<Language>(lang);
			pointer.ObjectId = "123";

			string resultJson = pointer.ToJson();

			Assert.That(resultJson, Is.EqualTo("{\"__type\":\"Pointer\",\"className\":\"language\",\"objectId\":\"123\"}"));
		}

		[Test]
		public void Pointer_Equals_SameValues_ReturnsTue()
		{
			var lang1 = new Language();
			var lang2 = new Language();

			var pointer1 = new Pointer<Language>(lang1);
			var pointer2 = new Pointer<Language>(lang2);

			bool result = pointer1.Equals(pointer2);

			Assert.That(result, Is.True);
		}

		[Test]
		public void Pointer_Equals_SameObjectIdButDifferentUnderlying_ReturnsTrue()
		{
			var lang1 = new Language();
			var lang2 = new Language();
			lang2.IsoCode = "es";

			var pointer1 = new Pointer<Language>(lang1);
			pointer1.ObjectId = "123";

			var pointer2 = new Pointer<Language>(lang2);
			pointer2.ObjectId = "123";

			// Well, this is a messed up, hypothetical world where both pointers claim to point to the same object, but actually have different underlying values
			// Well, a pointer just points to some reference... it's not the value.
			// I'd argue that pointer equality is sufficient if it points to the same reference.
			// So this case will return true

			bool result = pointer1.Equals(pointer2);

			Assert.That(result, Is.True);
		}

		[Test]
		public void Pointer_Equals_SameObjectIdButDifferentType_ReturnsFalse()
		{
			// Theoretically, it's hypothethically possible that pointers to different classes can happen to have the same objectId
			// These are not the same though, because they point into different classes.
			var pointer1 = new Pointer<Language>(null);
			pointer1.ObjectId = "123";

			var pointer2 = new Pointer<User>(null);
			pointer2.ObjectId = "123";

			bool result = pointer1.Equals(pointer2);

			Assert.That(result, Is.False);
		}

		[Test]
		public void Pointer_Equals_SameObjectIdButDifferentClass_ReturnsFalse()
		{
			// In this contrived scenario, we have two different Parse tables (aka classes)
			// but which have the same schema. So hypothetically, it's not completely out of the picture that
			// one could represent that using the same C# class (although... I think it's a bad idea)
			// Then you'd have this contrived scenario where the pointer has the same objectId, and the same C# type, and differ only in className
			// They are still different in this scenario though.
			var pointer1 = new Pointer<Language>(null);
			pointer1.ObjectId = "123";
			pointer1.ClassName = "languages";

			var pointer2 = new Pointer<Language>(null);
			pointer2.ObjectId = "123";
			pointer2.ClassName = "languagesDeprecated";

			bool result = pointer1.Equals(pointer2);

			Assert.That(result, Is.False);
		}

		[Test]
		public void Pointer_Equals_DifferentReferences_ReturnsFalse()
		{
			var lang = new Language();

			var pointer1 = new Pointer<Language>(lang);
			pointer1.ObjectId = "1";

			var pointer2 = new Pointer<Language>(lang);
			pointer2.ObjectId = "2";

			bool result = pointer1.Equals(pointer2);

			Assert.That(result, Is.False);
		}

		[Test]
		public void Pointer_Equals_NullPointer_NoException()
		{
			var pointer1 = new Pointer<Language>(new Language());
			bool result = pointer1.Equals(null);
			Assert.That(result, Is.False);
		}

		[Test]
		public void Pointer_Equals_NullValue_NoException()
		{
			var pointer1 = new Pointer<Language>(new Language());
			var pointer2 = new Pointer<Language>(null);

			bool result = pointer1.Equals(pointer2);

			Assert.That(result, Is.False);
		}
	}
}
